using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class ArchiveOverdueTasksJob
    {
        private readonly AppDbContext _db;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IGetHigherManager _getHigherManager;

        public ArchiveOverdueTasksJob(AppDbContext db, IDomainEventDispatcher eventDispatcher,IGetHigherManager getHigherManager
)
        {
            _db = db;
            _eventDispatcher = eventDispatcher;
            _getHigherManager = getHigherManager;
        }

        public async Task ExecuteAsync()
        {
            var overdueTasks = await _db.Tasks
                .Include(t => t.Assignments)
                .Where(t => t.DueDate < DateTime.UtcNow
                            && t.Status != WorkTaskStatus.Closed
                            && t.Status != WorkTaskStatus.Archived
                            && t.Status != WorkTaskStatus.AutoClose)
                .ToListAsync();

            if (!overdueTasks.Any()) return;

            foreach (var task in overdueTasks)
            {
                var approvedExtension = await _db.TaskExtensionRequests
                    .Where(r => r.TaskId == task.Id
                         && r.Status == ExtensionRequestStatus.Approved
                         && r.NewDueDate > DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (approvedExtension != null)
                    continue;

                task.Status = WorkTaskStatus.AutoClose;
                task.ClosedAt = DateTime.UtcNow;
                task.CloseReason = CloseReason.Auto;

                var assignmentsToClose = await _db.TaskAssignments
                    .Where(a => a.TaskId == task.Id)
                    .ToListAsync();

                foreach (var assignment in assignmentsToClose)
                {
                    assignment.IsClosed = true;
                }

                var discountsToPublish = new List<Discount>();


                var candidateIds = task.Assignments
                    .Select(a => a.EmployeeId)
                    .Distinct()
                    .ToList();

                if (task.CreatedByEmployeeId.HasValue)
                    candidateIds.Add(task.CreatedByEmployeeId.Value);

                candidateIds = candidateIds.Distinct().ToList();

                var roleLevels = await _db.EmployeeRoles
                    .Where(er => candidateIds.Contains(er.EmployeeId)
                                 && er.IsAssigned
                                 && !er.IsDeleted
                                 && er.Role != null)
                    .GroupBy(er => er.EmployeeId)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        RoleLevel = g.Max(x => x.Role.Level)
                    })
                    .ToListAsync();

                var exemptIds = new HashSet<int>(
                    roleLevels.Where(x => x.RoleLevel == 100)
                              .Select(x => x.EmployeeId)
                );

                var level80Ids = roleLevels
                    .Where(x => x.RoleLevel == 80)
                    .Select(x => x.EmployeeId)
                    .ToList();

                if (level80Ids.Any())
                {
                    var ops80Ids = await _db.Employees
                        .Where(e => level80Ids.Contains(e.Id)
                                    && e.FunctionCode == FunctionCode.Operations)
                        .Select(e => e.Id)
                        .ToListAsync();

                    foreach (var id in ops80Ids)
                        exemptIds.Add(id);
                }

                var closeRequestIds = new HashSet<int>(
                    await _db.TaskCloseRequests
                    .Where(r => r.TaskId == task.Id
                    && r.RequestedBy != null
                    && candidateIds.Contains(r.RequestedBy.Id))
                    .Select(r => r.RequestedBy.Id)
                    .Distinct()
                    .ToListAsync()
                );

                // ✅ NEW BUSINESS:
                // لو المهمة Shared وحد قدم طلب اغلاق => مفيش خصم على أي حد على المهمة دي (ولا حتى creator)
                var skipAllDiscountsBecauseSharedCloseRequest =
                    task.IsShared && closeRequestIds.Any();

                if (!skipAllDiscountsBecauseSharedCloseRequest)
                {
                    foreach (var assignment in task.Assignments)
                    {
                        // Skip discount for exempt employees
                        if (exemptIds.Contains(assignment.EmployeeId))
                            continue;

                        // (مازال موجود زي ما هو، بس في حالة shared + close request احنا مش بندخل هنا أصلاً)
                        if (closeRequestIds.Contains(assignment.EmployeeId))
                            continue;

                        var alreadyDiscounted = await _db.Discounts.AnyAsync(d =>
                            d.TaskId == task.Id &&
                            d.EmployeeId == assignment.EmployeeId &&
                            d.discountType == DiscountType.AutoCloseTaskDiscount &&
                            d.AutoDiscount);

                        if (alreadyDiscounted)
                            continue;

                        var discount = new Discount
                        {
                            TaskId = task.Id,
                            EmployeeId = assignment.EmployeeId,
                            Reason = "add discount on auto close task",
                            Amount = task.PenaltyOnAutoClose,
                            //CreatedByEmployeeId = 0,
                            AutoDiscount = true,
                            CreatedDate = DateTime.UtcNow,
                            discountType = DiscountType.AutoCloseTaskDiscount
                        };

                        await _db.Discounts.AddAsync(discount);
                        discountsToPublish.Add(discount);
                    }


                    if (task.CreatedByEmployeeId.HasValue)
                    {
                        var creatorId = task.CreatedByEmployeeId.Value;

                        var creatorCommented = await _db.TaskComments.AnyAsync(c =>
                            c.TaskId == task.Id &&
                            c.EmployeeId == creatorId);

                        var creatorAddedPenalty = await _db.Discounts.AnyAsync(d =>
                            d.TaskId == task.Id &&
                            d.CreatedByEmployeeId == creatorId &&
                            !d.AutoDiscount);

                        var creatorAddedWarning = await _db.Warnings.AnyAsync(w =>
                            w.TaskId == task.Id &&
                            w.CreatedBy == creatorId);

                        var managerDidSomething = creatorCommented || creatorAddedPenalty || creatorAddedWarning;

                        if (!managerDidSomething)
                        {
                            // Skip discount for exempt creators
                            // Skip manager discount ONLY (do NOT skip saving the task changes)
                            if (!exemptIds.Contains(creatorId))
                            {
                                var alreadyDiscounted = await _db.Discounts.AnyAsync(d =>
                                    d.TaskId == task.Id &&
                                    d.discountType == DiscountType.AutoCloseTaskDiscount &&
                                    d.AutoDiscount);

                                if (!alreadyDiscounted)
                                {
                                    var managerDiscount = new Discount
                                    {
                                        TaskId = task.Id,
                                        EmployeeId = creatorId,
                                        Reason = "manager negligence on overdue task",
                                        Amount = task.PenaltyOnAutoClose,
                                        //CreatedByEmployeeId = 0,
                                        AutoDiscount = true,
                                        CreatedDate = DateTime.UtcNow,
                                        discountType = DiscountType.AutoCloseTaskDiscount
                                    };

                                    await _db.Discounts.AddAsync(managerDiscount);
                                    discountsToPublish.Add(managerDiscount);
                                }
                            }
                        }
                    }
                }

                await _db.SaveChangesAsync();

                foreach (var discount in discountsToPublish)
                {
                    try
                    {
                        var employeeName = await _db.Employees
                            .Where(e => e.Id == discount.CreatedByEmployeeId)
                            .Select(e => e.FullName)
                            .FirstOrDefaultAsync();

                        var issuedEmployee = await _db.Employees
                            .Where(e => e.Id == discount.EmployeeId)
                            .Select(e => new
                            {
                                e.Id,
                                e.FullName,
                                e.BranchId
                            })
                            .FirstOrDefaultAsync();

                        var branch = issuedEmployee.BranchId.HasValue
                            ? await _db.Branches
                                .Include(b => b.Manager)
                                .FirstOrDefaultAsync(b => b.Id == issuedEmployee.BranchId.Value)
                            : null;

                        var managerId = await _getHigherManager.GetDirectHigherManagerIdAsync(discount.EmployeeId);

                        var sendToIds = new List<int> { discount.EmployeeId };
                        if (managerId.HasValue && !sendToIds.Contains(managerId.Value))
                            sendToIds.Add(managerId.Value);

                        var issuedToName = issuedEmployee.FullName;

                        await _eventDispatcher.PublishAsync(
                            new TaskPenaltyEvent(
                                discount.Id,
                                task.Id,
                                employeeName ?? "",
                                sendToIds,
                                issuedToName,
                                task.Title
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        // intentionally ignored (same as original behavior)
                    }
                }
            }
        }
    }
}
