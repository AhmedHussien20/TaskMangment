using Microsoft.EntityFrameworkCore;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Domain.Event;

namespace TaskMangment.Hangfire.Jobs
{
    public class PenaltyForMissingCommentsJob
    {
        private readonly AppDbContext _db;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public PenaltyForMissingCommentsJob(AppDbContext db, IDomainEventDispatcher eventDispatcher)
        {
            _db = db;
            _eventDispatcher = eventDispatcher;
        }

        public async Task ExecuteAsync()
        {
            var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Arab Standard Time").Date;
            var yesterday = today.AddDays(-1);

            var tasks = await _db.Tasks
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Employee)
                .Where(t => t.CommentAllowPeriodDays != null && t.DueDate > yesterday)
                .Where(t =>
                    t.Status != WorkTaskStatus.Closed &&
                    t.Status != WorkTaskStatus.AutoClose &&
                    t.Status != WorkTaskStatus.Archived)
                //add duedate check
                .ToListAsync();

            var discountsToPublish = new List<(Discount discount, string employeeName, int taskId, string taskTitle)>();

            foreach (var task in tasks)
            {
                var periodDays = (int)task.CommentAllowPeriodDays!.Value;

                foreach (var assignment in task.Assignments)
                {
                    var employeeId = assignment.EmployeeId;

                    var lastComment = await _db.TaskComments
                        .Where(c => c.TaskId == task.Id && c.EmployeeId == employeeId)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefaultAsync();

                    var shouldHaveComment =
                        (lastComment == null && assignment.AssignedAt.Date.AddDays(periodDays) <= yesterday && assignment.IsActive)
                        || (lastComment != null && lastComment.CreatedDate.Date.AddDays(periodDays) <= yesterday);

                    if (!shouldHaveComment) continue;

                    var hasLeave = await _db.Leaves
                        .Where(l => l.EmployeeId == employeeId &&
                                    l.StartDate.Date <= yesterday &&
                                    l.EndDate.Date >= yesterday&&
                                    l.Status == LeaveStatus.Approved)
                        .AnyAsync();
                    if (hasLeave) continue;

                    if (task.PenaltyOnStopComment <= 0) continue;

                    var alreadyDiscounted = await _db.Discounts.AnyAsync(d =>
                        d.TaskId == task.Id &&
                        d.EmployeeId == employeeId &&
                        d.discountType == DiscountType.StopCommentDiscount &&
                        d.AutoDiscount &&
                        d.CreatedDate.Date == today);

                    if (alreadyDiscounted)
                        continue;

                    var discount = new Discount
                    {
                        TaskId = task.Id,
                        EmployeeId = employeeId,
                        Amount = task.PenaltyOnStopComment,
                        Reason = "Penalty for not commenting",
                        //CreatedByEmployeeId = 0,
                        AutoDiscount = true,
                        CreatedDate = DateTime.UtcNow,
                        discountType = DiscountType.StopCommentDiscount
                    };

                    await _db.Discounts.AddAsync(discount);

                    discountsToPublish.Add((discount, assignment.Employee?.FullName ?? "", task.Id, task.Title));
                }
            }

            await _db.SaveChangesAsync();

            foreach (var (discount, employeeName, taskId, taskTitle) in discountsToPublish)
            {
                try
                {
                    var issuedEmployee = await _db.Employees
                        .Where(e => e.Id == discount.EmployeeId)
                        .Select(e => new
                        {
                            e.Id,
                            e.FullName,
                            e.BranchId
                        })
                        .FirstOrDefaultAsync();

                    if (issuedEmployee == null) continue;

                    var branch = issuedEmployee.BranchId.HasValue
                        ? await _db.Branches
                            .Include(b => b.Manager)
                            .FirstOrDefaultAsync(b => b.Id == issuedEmployee.BranchId.Value)
                        : null;

                    var managerId = branch?.ManagerID;

                    var sendToIds = new List<int> { discount.EmployeeId };
                    if (managerId.HasValue && !sendToIds.Contains(managerId.Value))
                        sendToIds.Add(managerId.Value);

                    var issuedToName = issuedEmployee.FullName;

                    var employeeCreatedName = await _db.Employees
                        .Where(e => e.Id == discount.CreatedByEmployeeId)
                        .Select(e => e.FullName)
                        .FirstOrDefaultAsync() ?? "";

                    await _eventDispatcher.PublishAsync(
                        new TaskPenaltyEvent(
                            discount.Id,
                            taskId,
                            employeeCreatedName ?? "",
                            sendToIds,
                            issuedToName,
                            taskTitle
                        )
                    );
                }
                catch (Exception ex)
                {
                    
                }
            }

        }

    }
}
