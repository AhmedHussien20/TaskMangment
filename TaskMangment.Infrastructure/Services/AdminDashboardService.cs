
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TaskMangment.Application.Common.DiscountTypes;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Helpers;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Infrastructure.Services
{
    namespace TaskMangment.Infrastructure.Services.Dashboard
    {
        public class AdminDashboardService : IAdminDashboardService
        {
            private readonly IRepository<WorkTask> _taskRepo;
            private readonly IRepository<Employee> _employeeRepo;
            private readonly IRepository<Discount> _deductionRepo;
            private readonly IRepository<TaskAssignment> _assignmentRepo;
            private readonly IRepository<Warning> _warningRepo;
            private readonly IRepository<Attachment> _attachmentRepo;
            private readonly ICachingService _cache;
            private readonly IStringLocalizer<DiscountAutoType> _localizer;


            public AdminDashboardService(
                IRepository<WorkTask> taskRepo,
                IRepository<Employee> employeeRepo,
                IRepository<Discount> deductionRepo,
                IRepository<TaskAssignment> assignmentRepo,
                IRepository<Warning> warningRepo,
                IRepository<Attachment> attachmentRepo,
            ICachingService cache,
            IStringLocalizer<DiscountAutoType> localizer
            )
            {
                _taskRepo = taskRepo;
                _employeeRepo = employeeRepo;
                _deductionRepo = deductionRepo;
                _assignmentRepo = assignmentRepo;
                _warningRepo = warningRepo;
                _cache = cache;
                _assignmentRepo = assignmentRepo;
                _attachmentRepo = attachmentRepo;
                _localizer = localizer;

            }

            public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(
     int companyId,
     int roleLevel,           
     int? employeeId = null,   
     PeriodDto? period = null)
            {
                string cacheKey = $"dashboard:admin:{companyId}:{roleLevel}:{employeeId}";

                var cached = await _cache.GetAsync<AdminDashboardDto>(cacheKey);
                if (cached != null)
                    return ApiResponse<AdminDashboardDto>.Ok(cached);

                var range = PeriodHelper.GetRange(period);


                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                // ======================= Total Employees =======================
                var totalEmployeesQuery = _employeeRepo.GetAll(e => e.CompanyId == companyId && e.IsActive);

                if (roleLevel == 70 && branchId.HasValue)
                    totalEmployeesQuery = totalEmployeesQuery.Where(e => e.BranchId == branchId.Value);

                var totalEmployees = await totalEmployeesQuery.CountAsync();

                // ======================= Active Tasks =======================
                var activeTasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    activeTasksQuery = activeTasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value)
                    );
                }

                var activeTasks = await activeTasksQuery.CountAsync();

                // ======================= Overdue Tasks =======================
                var overdueTasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.Status != WorkTaskStatus.Closed &&
                    t.DueDate != null &&
                    t.DueDate < DateTime.Today &&
                    t.DueDate >= range.Start && t.DueDate <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    overdueTasksQuery = overdueTasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value)
                    );
                }

                var overdueTasks = await overdueTasksQuery.CountAsync();

                // ======================= Completed Tasks =======================
                var completedTasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.Status == WorkTaskStatus.Closed &&
                    t.ClosedAt >= range.Start && t.ClosedAt <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    completedTasksQuery = completedTasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value)
                    );
                }

                var completedTasks = await completedTasksQuery.CountAsync();

                // ======================= Penalties =======================
                var penaltiesQuery = _deductionRepo.GetAll(d =>
                    d.Employee.CompanyId == companyId &&
                    d.CreatedDate >= range.Start && d.CreatedDate <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                    penaltiesQuery = penaltiesQuery.Where(d =>
                        d.Employee.BranchId == branchId.Value ||
                        d.EmployeeId == employeeId.Value
                    );

                var penaltiesThisPeriod = await penaltiesQuery.SumAsync(d => d.Amount);

                // ======================= Warnings =======================
                var warningsQuery = _warningRepo.GetAll(w =>
                    w.Task.CompanyId == companyId &&
                    w.IssuedAt >= range.Start && w.IssuedAt <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                    warningsQuery = warningsQuery.Where(w =>
                        w.Task.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        w.Task.Assignments.Any(a => a.EmployeeId == employeeId.Value)
                    );

                var warningsThisPeriod = await warningsQuery.CountAsync();

                // ======================= Top Delayed Employees =======================
                var topDelayedQuery = _assignmentRepo.GetAll(a =>
                    a.Task.CompanyId == companyId &&
                    a.IsActive &&
                    a.Task.DueDate >= range.Start && a.Task.DueDate <= range.End &&
                    a.Task.Status != WorkTaskStatus.Closed);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                    topDelayedQuery = topDelayedQuery.Where(a =>
                        a.Employee.BranchId == branchId.Value ||
                        a.EmployeeId == employeeId.Value
                    );

                var topDelayedEmployees = await topDelayedQuery
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => new TopDelayedEmployeeDto
                    {
                        EmployeeId = g.Key,
                        EmployeeName = g.First().Employee.FullName,
                        DelayedTasksCount = g.Count()
                    })
                    .OrderByDescending(x => x.DelayedTasksCount)
                    .Take(5)
                    .ToListAsync();

                // ======================= DTO =======================
                var dto = new AdminDashboardDto
                {
                    Kpis = new AdminKpiDto
                    {
                        TotalEmployees = totalEmployees,
                        ActiveTasks = activeTasks,
                        OverdueTasks = overdueTasks,
                        CompletedTasks = completedTasks,
                        TotalPenaltiesThisMonth = penaltiesThisPeriod,
                        WarningsThisMonth = warningsThisPeriod
                    },
                    TopDelayedEmployees = topDelayedEmployees
                };

                // حفظ الكاش
                await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

                return ApiResponse<AdminDashboardDto>.Ok(dto);
            }



            public async Task<ApiResponse<List<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(
    int companyId,
    int roleLevel,
    int? employeeId,
    PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var tasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    (
                        t.Comments.Any(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End) ||
                        t.Assignments.Any(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                    ));

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value)
                    );
                }

                var tasks = await tasksQuery
                    .Select(t => new
                    {
                        Task = t,
                        LastComment = t.Comments
                            .Where(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End)
                            .OrderByDescending(c => c.CreatedDate)
                            .Select(c => new { c.CreatedDate, EmployeeName = c.Employee.FullName })
                            .FirstOrDefault(),
                        LastAssignmentUpdate = t.Assignments
                            .Where(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                            .OrderByDescending(a => a.ModifiedDate)
                            .Select(a => new { a.ModifiedDate, EmployeeName = a.Employee.FullName })
                            .FirstOrDefault()
                    })
                    .Select(x => new UpdatedTodayTaskDto
                    {
                        TaskId = x.Task.Id,
                        Title = x.Task.Title,
                        Status = x.Task.Status,
                        DueDate = x.Task.DueDate,
                        UpdatedAt = x.LastComment != null &&
                                    (x.LastAssignmentUpdate == null || x.LastComment.CreatedDate >= x.LastAssignmentUpdate.ModifiedDate)
                                    ? x.LastComment.CreatedDate
                                    : x.LastAssignmentUpdate.ModifiedDate,
                        UpdatedBy = x.LastComment != null &&
                                    (x.LastAssignmentUpdate == null || x.LastComment.CreatedDate >= x.LastAssignmentUpdate.ModifiedDate)
                                    ? x.LastComment.EmployeeName
                                    : x.LastAssignmentUpdate.EmployeeName
                    })
                    .OrderByDescending(t => t.UpdatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return ApiResponse<List<UpdatedTodayTaskDto>>.Ok(tasks);
            }



            public async Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(
     int companyId,
     int roleLevel,
     int? employeeId,
     PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var query = _assignmentRepo.GetAll(a =>
                    a.Task.CompanyId == companyId &&
                    a.Task.Status == WorkTaskStatus.Closed &&
                    a.ModifiedDate >= range.Start &&
                    a.ModifiedDate <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                    query = query.Where(a =>
                        a.Employee.BranchId == branchId.Value ||
                        a.EmployeeId == employeeId.Value);

                var result = await query
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => new CompletedTodayEmployeeDto
                    {
                        EmployeeId = g.Key,
                        EmployeeName = g.First().Employee.FullName,
                        CompletedTasksCount = g.Count()
                    })
                    .OrderByDescending(x => x.CompletedTasksCount)
                    .ToListAsync();

                return ApiResponse<List<CompletedTodayEmployeeDto>>.Ok(result);
            }


            public async Task<ApiResponse<List<PendingCloseRequestTaskDto>>> GetPendingCloseRequestsAsync(
    int companyId,
    int roleLevel,
    int? employeeId,
    PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var query = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.CloseRequests.Any(r =>
                        r.Status == CloseRequestStatus.Pending &&
                        r.CreatedDate >= range.Start &&
                        r.CreatedDate <= range.End));

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                    query = query.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));

                var result = await query
                    .Select(t => new PendingCloseRequestTaskDto
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        RequestedBy = t.CloseRequests
                            .Where(r => r.Status == CloseRequestStatus.Pending &&
                                        r.CreatedDate >= range.Start &&
                                        r.CreatedDate <= range.End)
                            .Select(r => r.RequestedBy.FullName)
                            .FirstOrDefault(),
                        RequestedAt = t.CloseRequests
                            .Where(r => r.Status == CloseRequestStatus.Pending &&
                                        r.CreatedDate >= range.Start &&
                                        r.CreatedDate <= range.End)
                            .Select(r => r.CreatedDate)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(x => x.RequestedAt)
                    .ToListAsync();

                return ApiResponse<List<PendingCloseRequestTaskDto>>.Ok(result);
            }

            public async Task<ApiResponse<List<TaskStatusDto>>> GetTasksByStatusAsync(
     int companyId,
     string status,
     int roleLevel,         
     int? employeeId,      
     PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var tasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    (
                        (status == "Active" &&
                            (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress))
                        ||
                        (status == "Overdue" &&
                            t.Status != WorkTaskStatus.Closed &&
                            t.DueDate != null &&
                            t.DueDate >= range.Start && t.DueDate <= range.End)
                        ||
                        (status == "Completed" &&
                            t.Status == WorkTaskStatus.Closed &&
                            t.ClosedAt >= range.Start && t.ClosedAt <= range.End)
                    ));

                // ======================= فلترة حسب الفرع والموظف لو الدور Branch Manager =======================
                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));
                }

                var tasks = await tasksQuery
                    .Select(t => new TaskStatusDto
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        DueDate = t.DueDate,
                        Status = t.Status,
                        StatusText = t.Status.ToString(),
                        Employees = t.Assignments.Where(a => a.IsActive).Select(a => a.Employee.FullName).ToList()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return ApiResponse<List<TaskStatusDto>>.Ok(tasks);
            }



            private async Task<string?> GetEmployeeImageAsync(int employeeId)
            {
                return await _attachmentRepo
                    .GetAll(a =>
                        a.AttachmentType == AttachmentType.Employee &&
                        a.ReferenceId == employeeId)
                    .OrderByDescending(a => a.CreatedDate)
                    .Select(a => a.FilePath)
                    .FirstOrDefaultAsync();
            }

            public async Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(
    int companyId,
    int roleLevel,      
    int? employeeId,    
    PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                // ======================= Closed Tasks =======================
                var closedTasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.ClosedAt != null &&
                    t.ClosedAt >= range.Start &&
                    t.ClosedAt <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    closedTasksQuery = closedTasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));
                }

                double averageCompletionHours = 0;
                if (await closedTasksQuery.AnyAsync())
                {
                    averageCompletionHours = await closedTasksQuery
                        .Select(t => EF.Functions.DateDiffHour(t.CreatedDate, t.ClosedAt!.Value))
                        .AverageAsync();
                }

                var totalClosedTasks = await closedTasksQuery.CountAsync();

                var manualAdminClosedTasks = await closedTasksQuery
                    .Where(t => t.CloseReason == CloseReason.Manual || t.CloseReason == CloseReason.Admin)
                    .CountAsync();

                var onTimeRatePercent = totalClosedTasks == 0 ? 0 : (manualAdminClosedTasks * 100.0) / totalClosedTasks;

                // ======================= High Priority Open Tasks =======================
                var highPriorityQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.Priority == TaskPriority.High &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    highPriorityQuery = highPriorityQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));
                }

                var highPriorityOpenTasks = await highPriorityQuery.CountAsync();

                // ======================= Penalties =======================
                var penaltiesQuery = _deductionRepo.GetAll(d =>
                    d.Employee.CompanyId == companyId &&
                    d.CreatedDate >= range.Start &&
                    d.CreatedDate <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    penaltiesQuery = penaltiesQuery.Where(d =>
                        d.Employee.BranchId == branchId.Value ||
                        d.EmployeeId == employeeId.Value);
                }

                var penalties = await penaltiesQuery.SumAsync(d => d.Amount);

                var dto = new AdminKpisExtendedDto
                {
                    AverageCompletionHours = Math.Round(averageCompletionHours, 2),
                    OnTimeRatePercent = Math.Round(onTimeRatePercent, 2),
                    HighPriorityOpenTasks = highPriorityOpenTasks,
                    PenaltiesThisMonth = penalties
                };

                return ApiResponse<AdminKpisExtendedDto>.Ok(dto);
            }


            public async Task<ApiResponse<List<DiscountGetDto>>> GetDiscountsAsync(
                int companyId,
                int roleLevel,          
                int? employeeId,      
                PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var discountsQuery = _deductionRepo.GetAll(d =>
                    d.Employee.CompanyId == companyId &&
                    d.CreatedDate >= range.Start &&
                    d.CreatedDate <= range.End &&
                    d.Amount > 0);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    discountsQuery = discountsQuery.Where(d =>
                        d.Employee.BranchId == branchId.Value ||
                        d.EmployeeId == employeeId.Value);
                }

                var discounts = await discountsQuery
                    .Select(d => new DiscountGetDto
                    {
                        EmployeeName = d.Employee.FullName,
                        TaskTitle = d.Task.Title,
                        CreatedDate = d.CreatedDate,
                        Reason = d.Reason,
                        Amount = d.Amount,
                        AutoDiscount = d.AutoDiscount,
                        DiscountType = d.discountType
                    })
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();

                foreach (var item in discounts)
                {
                    if (!item.AutoDiscount)
                        continue;

                    item.Reason = GetAutoDiscountReason(item.DiscountType);
                }

                return ApiResponse<List<DiscountGetDto>>.Ok(discounts);
            }

            private string GetAutoDiscountReason(DiscountType type)
            {
                return type switch
                {
                    DiscountType.AutoCloseTaskDiscount =>
                        _localizer[DiscountTypes.AutoCloseTaskDiscount],

                    DiscountType.MaxWarningDiscount =>
                        _localizer[DiscountTypes.MaxwarningTaskDiscount],

                    DiscountType.StopCommentDiscount =>
                        _localizer[DiscountTypes.StopCommentTaskDiscount],

                    _ => string.Empty
                };
            }


            public async Task<ApiResponse<List<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(
    int companyId,
    int roleLevel,      
    int? employeeId)    
            {
                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var tasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.Priority == TaskPriority.High &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));
                }

                var tasks = await tasksQuery
                    .Select(t => new HighPriorityTaskDto
                    {
                        TaskTitle = t.Title,
                        Employees = t.Assignments.Where(a => a.IsActive).Select(a => a.Employee.FullName).ToList(),
                        Status = t.Status,
                        StatusText = t.Status.ToString(),
                        DueDate = t.DueDate
                    })
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();

                return ApiResponse<List<HighPriorityTaskDto>>.Ok(tasks);
            }


            public async Task<ApiResponse<List<CompletedTaskDetailDto>>> GetCompletedTasksDetailsAsync(
    int companyId,
    int roleLevel,       // 100 = Super Admin, 70 = Branch Manager
    int? employeeId,
    PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                int? branchId = null;
                if (roleLevel == 70 && employeeId.HasValue)
                {
                    var employee = await _employeeRepo.GetByIDAsync(employeeId.Value);
                    branchId = employee?.BranchId;
                }

                var tasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.ClosedAt != null &&
                    t.ClosedAt >= range.Start &&
                    t.ClosedAt <= range.End);

                if (roleLevel == 70 && branchId.HasValue && employeeId.HasValue)
                {
                    tasksQuery = tasksQuery.Where(t =>
                        t.Assignments.Any(a => a.Employee.BranchId == branchId.Value) ||
                        t.Assignments.Any(a => a.EmployeeId == employeeId.Value));
                }

                var tasks = await tasksQuery
                    .Select(t => new CompletedTaskDetailDto
                    {
                        TaskTitle = t.Title,
                        EmployeeNames = t.Assignments
                                         .Where(a => a.IsActive)
                                         .Select(a => a.Employee.FullName)
                                         .ToList(),
                        CreatedAt = t.CreatedDate,
                        ClosedAt = t.ClosedAt.Value,
                        DurationHours = EF.Functions.DateDiffHour(t.CreatedDate, t.ClosedAt.Value)
                    })
                    .OrderByDescending(t => t.ClosedAt)
                    .ToListAsync();

                return ApiResponse<List<CompletedTaskDetailDto>>.Ok(tasks);
            }


        }
    }

}
