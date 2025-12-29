 
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Application.DTOs; 
using TaskMangment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaskMangment.Infrastructure.Services
{
    namespace TaskMangment.Infrastructure.Services.Dashboard
    {
        public class AdminDashboardService : IAdminDashboardService
        {
            private readonly IRepository<WorkTask> _taskRepo;
            private readonly IRepository<Employee> _employeeRepo;
            private readonly IRepository<Deduction> _deductionRepo;
            private readonly IRepository<TaskAssignment> _assignmentRepo;
            private readonly IRepository<Warning> _warningRepo;
            private readonly IRepository<Attachment> _attachmentRepo;
            private readonly ICachingService _cache;

            public AdminDashboardService(
                IRepository<WorkTask> taskRepo,
                IRepository<Employee> employeeRepo,
                IRepository<Deduction> deductionRepo,
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

            public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId)
            {
                string cacheKey = $"dashboard:admin:{companyId}";

                var cached = await _cache.GetAsync<AdminDashboardDto>(cacheKey);
                if (cached != null)
                    return ApiResponse<AdminDashboardDto>.Ok(cached);

                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var totalEmployees = await _employeeRepo.CountAsync(e =>
                    e.CompanyId == companyId && e.IsActive);

                var activeTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                var overdueTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    t.Status != WorkTaskStatus.Closed &&
                    t.DueDate != null &&
                    t.DueDate < now);

                var completedTasks = await _taskRepo.CountAsync(t =>
                    t.CompanyId == companyId &&
                    t.Status == WorkTaskStatus.Closed);

                var penaltiesThisMonth = await _deductionRepo
                    .GetAll(d => d.Employee.CompanyId == companyId && d.CreatedDate >= monthStart)
                    .SumAsync(d => d.Amount);

                var warningsThisMonth = await _warningRepo.CountAsync(w =>
                    w.Task.CompanyId == companyId &&
                    w.IssuedAt >= monthStart);

                var topDelayedEmployees = await _assignmentRepo
                    .GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.IsActive &&
                        a.Task.DueDate < now &&
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
                        TotalPenaltiesThisMonth = penaltiesThisMonth,
                        WarningsThisMonth = warningsThisMonth
                    },
                    TopDelayedEmployees = topDelayedEmployees
                };

                await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

                return ApiResponse<AdminDashboardDto>.Ok(dto);
            }

            public async Task<ApiResponse<List<UpdatedTodayTaskDto>>>GetTodayUpdatedInProgressTasksAsync(int companyId)
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var tasks = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        //t.Status == WorkTaskStatus.InProgress &&
                        (
                            t.Comments.Any(c =>
                                c.CreatedDate >= today && c.CreatedDate < tomorrow)
                            ||
                            t.Assignments.Any(a =>
                                a.ModifiedDate >= today && a.ModifiedDate < tomorrow)
                        ))
                    .Select(t => new
                    {
                        Task = t,

                        LastComment = t.Comments
                            .Where(c => c.CreatedDate >= today && c.CreatedDate < tomorrow)
                            .OrderByDescending(c => c.CreatedDate)
                            .Select(c => new
                            {
                                c.CreatedDate,
                                EmployeeName = c.Employee.FullName
                            })
                            .FirstOrDefault(),

                        LastAssignmentUpdate = t.Assignments
                            .Where(a => a.ModifiedDate >= today && a.ModifiedDate < tomorrow)
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

                        UpdatedAt =
                            x.LastComment != null &&
                            (x.LastAssignmentUpdate == null ||
                             x.LastComment.CreatedDate >= x.LastAssignmentUpdate.ModifiedDate)
                                ? x.LastComment.CreatedDate
                                : x.LastAssignmentUpdate.ModifiedDate,

                        UpdatedBy =
                            x.LastComment != null &&
                            (x.LastAssignmentUpdate == null ||
                             x.LastComment.CreatedDate >= x.LastAssignmentUpdate.ModifiedDate)
                                ? x.LastComment.EmployeeName
                                : x.LastAssignmentUpdate.EmployeeName
                    })
                    .OrderByDescending(t => t.UpdatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return ApiResponse<List<UpdatedTodayTaskDto>>.Ok(tasks);
            }


            public async Task<ApiResponse<List<CompletedTodayEmployeeDto>>>GetEmployeesCompletedTasksTodayAsync(int companyId)
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var result = await _assignmentRepo
                    .GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.Task.Status == WorkTaskStatus.Closed &&
                        a.ModifiedDate >= today &&
                        a.ModifiedDate < tomorrow)
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

            public async Task<ApiResponse<List<PendingCloseRequestTaskDto>>>GetPendingCloseRequestsAsync(int companyId)
            {
                var result = await _taskRepo
                    .GetAll(t =>
                        t.CompanyId == companyId &&
                        t.CloseRequests.Any(r => r.Status == CloseRequestStatus.Pending))
                    .Select(t => new PendingCloseRequestTaskDto
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        RequestedBy = t.CloseRequests
                            .Where(r => r.Status == CloseRequestStatus.Pending)
                            .Select(r => r.RequestedBy.FullName)
                            .FirstOrDefault(),
                        RequestedAt = t.CloseRequests
                            .Where(r => r.Status == CloseRequestStatus.Pending)
                            .Select(r => r.CreatedDate)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(x => x.RequestedAt)
                    .ToListAsync();

                return ApiResponse<List<PendingCloseRequestTaskDto>>.Ok(result);
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

        }
    }

}
