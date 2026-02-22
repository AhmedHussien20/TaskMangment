using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Entities.Enum;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Hangfire.Jobs
{
    public class PenaltyForMissingCommentsJob
    {
        private readonly AppDbContext _db;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IGetHigherManager _getHigherManager;

        public PenaltyForMissingCommentsJob(
            AppDbContext db,
            IDomainEventDispatcher eventDispatcher,
            IGetHigherManager getHigherManager)
        {
            _db = db;
            _eventDispatcher = eventDispatcher;
            _getHigherManager = getHigherManager;
        }

        public async Task ExecuteAsync()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            var yesterday = today.AddDays(-1);

            // الجمعة مفيش خصومات
            if (yesterday.DayOfWeek == DayOfWeek.Friday)
                return;

            // أجازة رسمية
            var isPublicHoliday = await _db.CalendarEvents
                .AsNoTracking()
                .AnyAsync(e =>
                    e.Public == true &&
                    e.EventType == CalendarEventType.Holiday &&
                    !e.IsDeleted &&
                    e.StartDate.Date <= yesterday &&
                    (e.EndDate == null ? e.StartDate.Date >= yesterday : e.EndDate.Value.Date >= yesterday)
                );

            if (isPublicHoliday)
                return;

            var tasks = await _db.Tasks
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Employee)
                .Where(t => t.CommentAllowPeriodDays != null && t.DueDate > yesterday && !t.IsDeleted)
                .Where(t =>
                    t.Status != WorkTaskStatus.Closed &&
                    t.Status != WorkTaskStatus.AutoClose &&
                    t.Status != WorkTaskStatus.Archived)
                .ToListAsync();

            var discountsToPublish = new List<(Discount discount, string employeeName, int taskId, string taskTitle)>();

            foreach (var task in tasks)
            {
                if (task.PenaltyOnStopComment <= 0)
                    continue;

                var periodDays = (int)task.CommentAllowPeriodDays!.Value;
                if (periodDays < 1) periodDays = 1;

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

                foreach (var assignment in task.Assignments)
                {
                    if (!assignment.IsActive)
                        continue;

                    var employeeId = assignment.EmployeeId;

                    if (exemptIds.Contains(employeeId))
                        continue;

                    var lastComment = await _db.TaskComments
                        .Where(c => c.TaskId == task.Id && c.EmployeeId == employeeId && !c.IsDeleted)
                        .OrderByDescending(c => c.CreatedDate)
                        .FirstOrDefaultAsync();

                    var baseDate = lastComment == null
                        ? assignment.AssignedAt.Date
                        : lastComment.CreatedDate.Date;

                    var dueDate = (periodDays == 1 && lastComment != null)
                        ? baseDate.AddDays(periodDays)     
                        : baseDate.AddDays(periodDays - 1); 

                    if (dueDate > yesterday)
                        continue;

                    // ✅ بدل AnyAsync
                    if (lastComment != null && lastComment.CreatedDate.Date == dueDate)
                        continue;


                    var hasLeave = await _db.Leaves
                      .Where(l => l.EmployeeId == employeeId &&
                                  l.StartDate.Date <= yesterday &&
                                  l.EndDate.Date >= yesterday &&
                                  l.Status == LeaveStatus.Approved)
                      .AnyAsync();

                    if (hasLeave)
                        continue;

                    if (today.Day == 1)
                    {
                        var isAccountant = await _db.Employees
                            .Where(e => e.Id == employeeId)
                            .Select(e => e.FunctionCode == FunctionCode.Accounting) 
                            .FirstOrDefaultAsync();

                        if (isAccountant)
                        {
                            var localStart = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Unspecified);
                            var localEnd = new DateTime(today.Year, today.Month, today.Day, 8, 0, 0, DateTimeKind.Unspecified);

                            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
                            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

                            var commentedTodayBefore8 = await _db.TaskComments
                                .AnyAsync(c =>
                                    c.TaskId == task.Id &&
                                    c.EmployeeId == employeeId &&
                                    c.CreatedDate >= utcStart &&
                                    c.CreatedDate < utcEnd);

                            if (commentedTodayBefore8)
                                continue; 
                        }
                    }

                  

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
                        AutoDiscount = true,
                        CreatedDate = DateTime.UtcNow,
                        discountType = DiscountType.StopCommentDiscount,
                        ViolationDate = dueDate
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

                    var managerId = await _getHigherManager.GetDirectHigherManagerIdAsync(discount.EmployeeId);

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
                catch
                {
                    // intentionally ignored (same as original behavior)
                }
            }
        }
    }
}
