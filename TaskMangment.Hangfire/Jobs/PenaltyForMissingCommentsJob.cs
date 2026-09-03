using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Validation;
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
        private const string StopCommentWarningReason = "عدم التعليق في فترة السماح";

        private readonly AppDbContext _db;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IGetHigherManager _getHigherManager;
        private readonly IConfiguration _configuration;

        public PenaltyForMissingCommentsJob(
            AppDbContext db,
            IDomainEventDispatcher eventDispatcher,
            IGetHigherManager getHigherManager,
            IConfiguration configuration)
        {
            _db = db;
            _eventDispatcher = eventDispatcher;
            _getHigherManager = getHigherManager;
            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            if (HangfireQuietHours.ShouldSkipNow(_configuration))
                return;

            var tz = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
            var yesterday = today.AddDays(-1);

            // do not add discount in friday and saturday
            if (yesterday.DayOfWeek == DayOfWeek.Friday || yesterday.DayOfWeek == DayOfWeek.Saturday)
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
            var warningsToPublish = new List<(Warning warning, string employeeName, int taskId, string taskTitle)>();

            foreach (var task in tasks)
            {
              
                bool hasCloseRequest = await _db.TaskCloseRequests
                    .AnyAsync(r => r.TaskId == task.Id);

                if (hasCloseRequest)
                    continue;

                var periodDays = (int)task.CommentAllowPeriodDays!.Value;
                if (periodDays < 1) periodDays = 1;

                var minCommentsRequired = task.MinCommentsPerPeriod;
                if (minCommentsRequired < 1) minCommentsRequired = 1;

                var maxWarningsBeforeDiscount = task.MaxWarningsBeforeDiscount;
                if (maxWarningsBeforeDiscount < 0) maxWarningsBeforeDiscount = 0;
                if (maxWarningsBeforeDiscount > 3) maxWarningsBeforeDiscount = 3;

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

                foreach (var assignment in task.Assignments)
                {
                    if (!assignment.IsActive)
                        continue;
                    if (assignment.Employee == null || !assignment.Employee.IsActive)
                        continue;

                    var employeeId = assignment.EmployeeId;

                    if (exemptIds.Contains(employeeId))
                        continue;

                    // Rolling allow-period: when the employee completes the minimum
                    // (even late), the next period starts from that completion date.
                    // Example: deadline 2 Aug, 3rd comment on 5 Aug → next last chance = 12 Aug.
                    var commentDates = await _db.TaskComments
                        .Where(c =>
                            c.TaskId == task.Id &&
                            c.EmployeeId == employeeId &&
                            !c.IsDeleted)
                        .Select(c => c.CreatedDate)
                        .ToListAsync();

                    // Already-handled miss deadlines (one sanction per last-chance date).
                    // Used to advance the cycle so consecutive misses get their own warning.
                    var sanctionedViolationDates = await _db.Warnings
                        .Where(w =>
                            w.TaskId == task.Id &&
                            w.IssuedEmployeeId == employeeId &&
                            w.AutoWarning &&
                            !w.IsDeleted &&
                            w.ViolationDate != null)
                        .Select(w => w.ViolationDate!.Value.Date)
                        .ToListAsync();

                    var discountedViolationDates = await _db.Discounts
                        .Where(d =>
                            d.TaskId == task.Id &&
                            d.EmployeeId == employeeId &&
                            d.discountType == DiscountType.StopCommentDiscount &&
                            d.AutoDiscount &&
                            !d.IsDeleted)
                        .Select(d => d.ViolationDate.Date)
                        .ToListAsync();

                    var handledLastChanceDates = sanctionedViolationDates
                        .Concat(discountedViolationDates)
                        .ToHashSet();

                    if (!TryGetOpenCommentCycle(
                            assignment.AssignedAt.Date,
                            periodDays,
                            minCommentsRequired,
                            commentDates,
                            today,
                            yesterday,
                            handledLastChanceDates,
                            out var lastChanceDate,
                            out _))
                    {
                        // Still inside allow period, or minimum already met for current cycle.
                        continue;
                    }

                    var hasLeave = await _db.Leaves
                      .Where(l => !l.IsDeleted &&
                                  l.EmployeeId == employeeId &&
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
                            .Select(e => e.EmployeeType != null && e.EmployeeType.Code == EmployeeTypeCodes.Accounting)
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

                    // ViolationDate = the allow-period last-chance day the employee missed.
                    // One warning or discount per that date — never stack daily for the same miss.
                    var violationDay = lastChanceDate.Date;

                    if (handledLastChanceDates.Contains(violationDay))
                        continue;

                    // Ladder is across the whole task assignment (all missed cycles),
                    // not reset per last-chance date.
                    var totalAutoWarnings = await _db.Warnings.CountAsync(w =>
                        w.TaskId == task.Id &&
                        w.IssuedEmployeeId == employeeId &&
                        w.AutoWarning &&
                        !w.IsDeleted);

                    if (totalAutoWarnings < maxWarningsBeforeDiscount)
                    {
                        var warning = new Warning
                        {
                            TaskId = task.Id,
                            TaskAssignmentId = assignment.Id,
                            IssuedEmployeeId = employeeId,
                            Reason = StopCommentWarningReason,
                            IssuedAt = DateTime.UtcNow,
                            AutoWarning = true,
                            ViolationDate = violationDay
                        };

                        await _db.Warnings.AddAsync(warning);
                        handledLastChanceDates.Add(violationDay);
                        warningsToPublish.Add((warning, assignment.Employee?.FullName ?? "", task.Id, task.Title));
                        continue;
                    }

                    if (task.PenaltyOnStopComment <= 0)
                        continue;

                    var discount = new Discount
                    {
                        TaskId = task.Id,
                        EmployeeId = employeeId,
                        Amount = task.PenaltyOnStopComment,
                        Reason = "عدم التعليق فالحد المسموح",
                        AutoDiscount = true,
                        CreatedDate = DateTime.UtcNow,
                        discountType = DiscountType.StopCommentDiscount,
                        ViolationDate = violationDay
                    };

                    await _db.Discounts.AddAsync(discount);
                    handledLastChanceDates.Add(violationDay);
                    discountsToPublish.Add((discount, assignment.Employee?.FullName ?? "", task.Id, task.Title));
                }
            }

            await _db.SaveChangesAsync();

            foreach (var (warning, employeeName, taskId, taskTitle) in warningsToPublish)
            {
                try
                {
                    var issuedEmployee = await _db.Employees
                        .Where(e => e.Id == warning.IssuedEmployeeId)
                        .Select(e => new
                        {
                            e.Id,
                            e.FullName,
                            e.BranchId
                        })
                        .FirstOrDefaultAsync();

                    if (issuedEmployee == null) continue;

                    var branchName = issuedEmployee.BranchId.HasValue
                        ? await _db.Branches
                            .Where(b => b.Id == issuedEmployee.BranchId.Value)
                            .Select(b => b.Name)
                            .FirstOrDefaultAsync()
                        : null;

                    var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(warning.IssuedEmployeeId!.Value);

                    var sendToIds = new List<int> { warning.IssuedEmployeeId!.Value };
                    sendToIds.AddRange(managerIds.Where(id => !sendToIds.Contains(id)));
                    sendToIds = await _db.Employees
                        .Where(e => sendToIds.Contains(e.Id) && e.IsActive && !e.IsDeleted)
                        .Select(e => e.Id)
                        .ToListAsync();
                    if (!sendToIds.Any()) continue;

                    await _eventDispatcher.PublishAsync(
                        new TaskWarningEvent(
                            warning.Id,
                            taskId,
                            "النظام",
                            sendToIds,
                            issuedEmployee.FullName,
                            taskTitle,
                            branchName
                        )
                    );
                }
                catch
                {
                    // intentionally ignored (same as original behavior)
                }
            }

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

                    var branchName = issuedEmployee.BranchId.HasValue
                        ? await _db.Branches
                            .Where(b => b.Id == issuedEmployee.BranchId.Value)
                            .Select(b => b.Name)
                            .FirstOrDefaultAsync()
                        : null;

                    var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(discount.EmployeeId);

                    var sendToIds = new List<int> { discount.EmployeeId };
                    sendToIds.AddRange(managerIds.Where(id => !sendToIds.Contains(id)));
                    sendToIds = await _db.Employees
                        .Where(e => sendToIds.Contains(e.Id) && e.IsActive && !e.IsDeleted)
                        .Select(e => e.Id)
                        .ToListAsync();
                    if (!sendToIds.Any()) continue;

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
                            taskTitle,
                            discount.Amount,
                            branchName
                        )
                    );
                }
                catch
                {
                    // intentionally ignored (same as original behavior)
                }
            }
        }

        /// <summary>
        /// Resolves the current comment allow-cycle.
        /// Returns true only when the cycle deadline has passed (as of yesterday)
        /// and the employee is still under the minimum — i.e. should warn/penalize.
        /// When the minimum is met (on time or late), the next cycle starts from that
        /// completion date (next last-chance = completionDate + periodDays).
        /// Cycles already sanctioned (warning/discount for that last-chance date) are
        /// treated as closed so the next miss can get its own sanction.
        /// </summary>
        private static bool TryGetOpenCommentCycle(
            DateTime assignedAt,
            int periodDays,
            int minCommentsRequired,
            List<DateTime> commentDates,
            DateTime today,
            DateTime yesterday,
            HashSet<DateTime> handledLastChanceDates,
            out DateTime lastChanceDate,
            out int commentCountInCycle)
        {
            lastChanceDate = default;
            commentCountInCycle = 0;

            if (periodDays < 1) periodDays = 1;
            if (minCommentsRequired < 1) minCommentsRequired = 1;

            var orderedDates = commentDates
                .Select(d => d.Date)
                .OrderBy(d => d)
                .ToList();

            var cycleStart = assignedAt.Date;
            var startExclusive = false; // first cycle includes assignment day

            // Advance through completed cycles (minimum already met, or already sanctioned).
            while (true)
            {
                lastChanceDate = TaskDueDateRules.MoveToNextWorkday(
                    cycleStart.AddDays(periodDays));

                var datesInCycle = orderedDates
                    .Where(d =>
                        d <= today &&
                        (startExclusive ? d > cycleStart : d >= cycleStart))
                    .ToList();

                commentCountInCycle = datesInCycle.Count;

                if (commentCountInCycle >= minCommentsRequired)
                {
                    // Nth comment that completed this cycle → next cycle starts there.
                    var completionDate = datesInCycle[minCommentsRequired - 1];
                    cycleStart = completionDate;
                    startExclusive = true;
                    continue;
                }

                // Already warned/discounted for this last-chance day → close cycle and move on
                // so consecutive missed days each get one sanction of their own.
                if (handledLastChanceDates.Contains(lastChanceDate.Date) &&
                    yesterday >= lastChanceDate.Date)
                {
                    cycleStart = lastChanceDate.Date;
                    startExclusive = true;
                    continue;
                }

                break;
            }

            // Still inside allow period (deadline not reached yet as of yesterday).
            if (yesterday < lastChanceDate)
                return false;

            // Deadline passed and still under minimum → sanction.
            return commentCountInCycle < minCommentsRequired;
        }
    }
}
