using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class ArchiveOverdueTasksJob
    {
        private readonly AppDbContext _db;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IGetHigherManager _getHigherManager;

        public ArchiveOverdueTasksJob(AppDbContext db, IDomainEventDispatcher eventDispatcher, IGetHigherManager getHigherManager)
        {
            _db = db;
            _eventDispatcher = eventDispatcher;
            _getHigherManager = getHigherManager;
        }

        public async Task ExecuteAsync()
        {
            // End-of-day rule (Saudi calendar): due date 27 Jul stays open all of 27 Jul;
            // it becomes overdue only when local date is 28 Jul (job runs at 00:01).
            var tz = TimeZoneHelper.GetSaudiArabia();
            var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;

            var overdueTasks = await _db.Tasks
                .Include(t => t.Assignments)
                .Where(t => t.DueDate.HasValue
                            && t.DueDate.Value.Date < todayLocal
                            && t.Status != WorkTaskStatus.Closed
                            && t.Status != WorkTaskStatus.Archived
                            && t.Status != WorkTaskStatus.AutoClose && !t.IsDeleted)
                .ToListAsync();

            if (!overdueTasks.Any()) return;

            foreach (var task in overdueTasks)
            {
                bool hasCloseRequest = await _db.TaskCloseRequests
                   .AnyAsync(r => r.TaskId == task.Id);

                if (hasCloseRequest)
                    continue;

                // Approved extension keeps the task open through the full NewDueDate day.
                var approvedExtension = await _db.TaskExtensionRequests
                    .Where(r => r.TaskId == task.Id
                         && r.Status == ExtensionRequestStatus.Approved
                         && !r.IsDeleted
                         && r.NewDueDate.Date >= todayLocal)
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

                if (task.PenaltyOnAutoClose > 0)
                {
                    var candidateIds = task.Assignments
                        .Select(a => a.EmployeeId)
                        .Distinct()
                        .ToList();

                    if (task.CreatedByEmployeeId.HasValue)
                        candidateIds.Add(task.CreatedByEmployeeId.Value);

                    candidateIds = candidateIds.Distinct().ToList();

                    var companyWideEmployeeIds = await (
                        from er in _db.EmployeeRoles
                        where candidateIds.Contains(er.EmployeeId)
                              && er.IsAssigned && !er.IsDeleted
                              && er.Role != null && !er.Role.IsDeleted
                        from rp in er.Role.RolePermissions
                        where rp.IsAssigned && !rp.IsDeleted
                              && rp.Permission != null && !rp.Permission.IsDeleted
                              && (rp.Permission.Code == "VIEW_COMPANY_TASKS"
                                  || rp.Permission.Code == "VIEW_COMPANY_TASKS"
                                  || rp.Permission.Code == "RECEIVE_ORG_ESCALATIONS")
                        select er.EmployeeId
                    ).Distinct().ToListAsync();

                    var legacyAdminIds = await _db.EmployeeRoles
                        .Where(er => candidateIds.Contains(er.EmployeeId)
                                     && er.IsAssigned
                                     && !er.IsDeleted
                                     && er.Role != null
                                     && er.Role.Level == 100)
                        .Select(er => er.EmployeeId)
                        .Distinct()
                        .ToListAsync();

                    var exemptIds = new HashSet<int>(companyWideEmployeeIds.Concat(legacyAdminIds));

                    //var level80Ids = roleLevels
                    //    .Where(x => x.RoleLevel == 80)
                    //    .Select(x => x.EmployeeId)
                    //    .ToList();

                    //if (level80Ids.Any())
                    //{
                    //    var ops80Ids = await _db.Employees
                    //        .Where(e => level80Ids.Contains(e.Id)
                    //                    && e.EmployeeType != null && e.EmployeeType.Code == EmployeeTypeCodes.Operations)
                    //        .Select(e => e.Id)
                    //        .ToListAsync();

                    //    foreach (var id in ops80Ids)
                    //        exemptIds.Add(id);
                    //}

                    var closeRequestIds = new HashSet<int>(
                        await _db.TaskCloseRequests
                        .Where(r => r.TaskId == task.Id
                                    && r.RequestedBy != null
                                    && candidateIds.Contains(r.RequestedBy.Id))
                        .Select(r => r.RequestedBy.Id)
                        .Distinct()
                        .ToListAsync()
                    );

                    var skipAllDiscountsBecauseSharedCloseRequest =
                        task.IsShared && closeRequestIds.Any();

                    if (!skipAllDiscountsBecauseSharedCloseRequest)
                    {
                        foreach (var assignment in task.Assignments)
                        {
                            if (exemptIds.Contains(assignment.EmployeeId))
                                continue;

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
                                Reason = "خصم نتيجة الاغلاق التلقائي للمهمة",
                                Amount = task.PenaltyOnAutoClose,
                                AutoDiscount = true,
                                CreatedDate = DateTime.UtcNow,
                                ViolationDate = DateTime.UtcNow,
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

                            if (!managerDidSomething && !exemptIds.Contains(creatorId))
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
                                        Reason = "إهمال جهة التكليف في متابعة مهمة متأخرة",
                                        Amount = task.PenaltyOnAutoClose,
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

                        var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(discount.EmployeeId);

                        var sendToIds = new List<int> { discount.EmployeeId };
                        sendToIds.AddRange(managerIds.Where(id => !sendToIds.Contains(id)));

                        var issuedToName = issuedEmployee.FullName;

                        await _eventDispatcher.PublishAsync(
                            new TaskPenaltyEvent(
                                discount.Id,
                                task.Id,
                                employeeName ?? "",
                                sendToIds,
                                issuedToName,
                                task.Title,
                                discount.Amount
                            )
                        );
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}