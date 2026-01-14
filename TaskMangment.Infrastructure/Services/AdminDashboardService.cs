
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Helpers;

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

            public AdminDashboardService(
                IRepository<WorkTask> taskRepo,
                IRepository<Employee> employeeRepo,
                IRepository<Discount> deductionRepo,
                IRepository<TaskAssignment> assignmentRepo,
                IRepository<Warning> warningRepo,
                IRepository<Attachment> attachmentRepo,
            ICachingService cache)
            {
                _taskRepo = taskRepo;
                _employeeRepo = employeeRepo;
                _deductionRepo = deductionRepo;
                _assignmentRepo = assignmentRepo;
                _warningRepo = warningRepo;
                _cache = cache;
                _assignmentRepo = assignmentRepo;
            }

            public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId, PeriodDto? period = null)
            {
                string cacheKey = $"dashboard:admin:{companyId}";

                var cached = await _cache.GetAsync<AdminDashboardDto>(cacheKey);
                if (cached != null)
                    return ApiResponse<AdminDashboardDto>.Ok(cached);

                var range = PeriodHelper.GetRange(period);

                var totalEmployees = await _employeeRepo.CountAsync(e =>
                    e.CompanyId == companyId && e.IsActive);

                var activeTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                var overdueTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    t.Status != WorkTaskStatus.Closed &&
                    t.DueDate != null 
                   );

                var completedTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    t.Status == WorkTaskStatus.Closed &&
                    t.ClosedAt >= range.Start && t.ClosedAt <= range.End);

                var penaltiesThisPeriod = await _deductionRepo
                    .GetAll(d => d.Employee.CompanyId == companyId &&
                                 d.CreatedDate >= range.Start && d.CreatedDate <= range.End)
                    .SumAsync(d => d.Amount);

                var warningsThisPeriod = await _warningRepo.CountAsync(w =>
                    w.Task.CompanyId == companyId &&
                    w.IssuedAt >= range.Start && w.IssuedAt <= range.End);

                var topDelayedEmployees = await _assignmentRepo
                    .GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.IsActive &&
                        a.Task.DueDate >= range.Start && a.Task.DueDate <= range.End &&
                        a.Task.Status != WorkTaskStatus.Closed)
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

                await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

                return ApiResponse<AdminDashboardDto>.Ok(dto);
            }


            public async Task<ApiResponse<List<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var tasks = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        (
                            t.Comments.Any(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End)
                            ||
                            t.Assignments.Any(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                        ))
                    .Select(t => new
                    {
                        Task = t,
                        LastComment = t.Comments
                            .Where(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End)
                            .OrderByDescending(c => c.CreatedDate)
                            .Select(c => new
                            {
                                c.CreatedDate,
                                EmployeeName = c.Employee.FullName
                            })
                            .FirstOrDefault(),
                        LastAssignmentUpdate = t.Assignments
                            .Where(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                            .OrderByDescending(a => a.ModifiedDate)
                            .Select(a => new
                            {
                                a.ModifiedDate,
                                EmployeeName = a.Employee.FullName
                            })
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


            public async Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var result = await _assignmentRepo
                    .GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.Task.Status == WorkTaskStatus.Closed &&
                        a.ModifiedDate >= range.Start &&
                        a.ModifiedDate <= range.End)
                    .GroupBy(a => a.EmployeeId)
                    .Select(g => new CompletedTodayEmployeeDto
                    {
                        EmployeeId = g.Key,
                        EmployeeName = g.First().Employee.FullName,
                        CompletedTasksCount = g.Count(),
                    })
                    .OrderByDescending(x => x.CompletedTasksCount)
                    .ToListAsync();

                return ApiResponse<List<CompletedTodayEmployeeDto>>.Ok(result);
            }


            public async Task<ApiResponse<List<PendingCloseRequestTaskDto>>> GetPendingCloseRequestsAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var result = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        t.CloseRequests.Any(r =>
                            r.Status == CloseRequestStatus.Pending &&
                            r.CreatedDate >= range.Start &&
                            r.CreatedDate <= range.End))
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
            public async Task<ApiResponse<List<TaskStatusDto>>> GetTasksByStatusAsync(int companyId, string status, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);
                var now = DateTime.UtcNow;

                var tasks = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        (
                            (status == "Active" &&
                                (t.Status == WorkTaskStatus.New ||
                                t.Status == WorkTaskStatus.InProgress
                                ))

                            ||
                            (status == "Overdue" &&
                                t.Status != WorkTaskStatus.Closed &&
                                t.DueDate != null &&
                                t.DueDate >= range.Start && t.DueDate <= range.End)
                            ||
                            (status == "Completed" &&
                                t.Status == WorkTaskStatus.Closed &&
                                t.ClosedAt >= range.Start && t.ClosedAt <= range.End)
                        ))
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

            public async Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var closedTasksQuery = _taskRepo.GetAll(t =>
                    t.CompanyId == companyId &&
                    t.ClosedAt != null &&
                    t.ClosedAt >= range.Start &&
                    t.ClosedAt <= range.End);

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

                var highPriorityOpenTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    t.Priority == TaskPriority.High &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                var penalties = await _deductionRepo
                    .GetAll(d =>
                        d.Employee.CompanyId == companyId &&
                        d.CreatedDate >= range.Start &&
                        d.CreatedDate <= range.End)
                    .SumAsync(d => d.Amount);

                var dto = new AdminKpisExtendedDto
                {
                    AverageCompletionHours = Math.Round(averageCompletionHours, 2),
                    OnTimeRatePercent = Math.Round(onTimeRatePercent, 2),
                    HighPriorityOpenTasks = highPriorityOpenTasks,
                    PenaltiesThisMonth = penalties
                };

                return ApiResponse<AdminKpisExtendedDto>.Ok(dto);
            }

            public async Task<ApiResponse<List<DiscountGetDto>>> GetDiscountsAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var discounts = await _deductionRepo
                    .GetAll(d =>
                        d.Employee.CompanyId == companyId &&
                        d.CreatedDate >= range.Start &&
                        d.CreatedDate <= range.End && d.Amount > 0)
                    .Select(d => new DiscountGetDto
                    {
                        EmployeeName = d.Employee.FullName,
                        TaskTitle = d.Task.Title,
                        CreatedDate = d.CreatedDate,
                        Reason = d.Reason,
                        Amount = d.Amount
                    })
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();

                return ApiResponse<List<DiscountGetDto>>.Ok(discounts);
            }


            public async Task<ApiResponse<List<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(int companyId)
            {
                var tasks = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        t.Priority == TaskPriority.High &&
                        (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress))
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

            public async Task<ApiResponse<List<CompletedTaskDetailDto>>> GetCompletedTasksDetailsAsync(int companyId, PeriodDto period)
            {
                var range = PeriodHelper.GetRange(period);

                var tasks = await _taskRepo
     .GetAll(t =>
         t.CompanyId == companyId &&
         t.ClosedAt != null &&
         t.ClosedAt >= range.Start &&
         t.ClosedAt <= range.End)
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
