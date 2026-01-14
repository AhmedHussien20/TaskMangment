using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Dashboards.Employee;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Helpers;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Discount> _deductionRepo;
        private readonly ICachingService _cache;

        public EmployeeDashboardService(
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<Warning> warningRepo,
            IRepository<Discount> deductionRepo,
            ICachingService cache)
        {
            _assignmentRepo = assignmentRepo;
            _warningRepo = warningRepo;
            _deductionRepo = deductionRepo;
            _cache = cache;
        }

        public async Task<ApiResponse<EmployeeDashboardDto>> GetDashboardAsync(int employeeId, PeriodDto? period = null)
        {
            string cacheKey = $"dashboard:employee:{employeeId}";

            var cached = await _cache.GetAsync<EmployeeDashboardDto>(cacheKey);
            if (cached != null)
                return ApiResponse<EmployeeDashboardDto>.Ok(cached);

            var range = PeriodHelper.GetRange(period);
            var now = DateTime.UtcNow;
            var dueSoonDate = now.AddDays(2);

            var activeTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsActive &&
                !a.IsClosed);

            var dueSoonTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsActive &&
                !a.IsClosed &&
                a.Task.DueDate != null &&
                a.Task.DueDate <= dueSoonDate);

            var warningsCount = await _warningRepo.CountAsync(w =>
                w.TaskAssignment.EmployeeId == employeeId&&
                w.CreatedDate >= range.Start && w.CreatedDate <= range.End
                );

            var penaltiesTotal = await _deductionRepo
                .GetAll(d => d.EmployeeId == employeeId&&
                  d.CreatedDate >= range.Start && d.CreatedDate <= range.End
                )
                .SumAsync(d => d.Amount);

            var myTasks = await _assignmentRepo
                .GetAll(a =>
                    a.EmployeeId == employeeId &&
                    a.IsActive && !a.IsClosed)
                .Select(a => new MyTaskDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    Status = a.Task.Status,
                    DueDate = a.Task.DueDate,
                    ProgressPercent = a.ProgressPercent
                })
                .OrderBy(a => a.DueDate)
                .Take(10)
                .ToListAsync();

            var completedTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId &&
                a.IsClosed);

            var totalTasks = await _assignmentRepo.CountAsync(a =>
                a.EmployeeId == employeeId);

            var dto = new EmployeeDashboardDto
            {
                Kpis = new EmployeeKpiDto
                {
                    MyActiveTasks = activeTasks,
                    DueSoonTasks = dueSoonTasks,
                    MyWarnings = warningsCount,
                    MyPenalties = penaltiesTotal
                },
                MyTasks = myTasks,
                Performance = new PerformanceSummaryDto
                {
                    CompletedTasks = completedTasks,
                    TotalTasks = totalTasks
                }
            };

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(3));

            return ApiResponse<EmployeeDashboardDto>.Ok(dto);
        }

        public async Task<ApiResponse<PagedResponse<TodayCommentTaskDto>>>GetTasksWithoutCommentsTodayAsync(BaseApiRequest request, int employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var allowedStatuses = new[] { WorkTaskStatus.New, WorkTaskStatus.InProgress };
            var query = _assignmentRepo.GetAll(a =>
                a.EmployeeId == employeeId &&
                a.IsActive &&
                !a.IsClosed &&
                allowedStatuses.Contains(a.Task.Status) &&   
                a.Task.Comments.All(c => c.CreatedDate < today)
            ).AsQueryable();

            query = query
                .Include(a => a.Task)
                    .ThenInclude(t => t.AssignedBy)
                .Include(a => a.Task)
                    .ThenInclude(t => t.Assignments)
                        .ThenInclude(ta => ta.Employee);

            if (!string.IsNullOrWhiteSpace(request.searchKey))
            {
                query = query.Where(a =>
                    a.Task.Title.Contains(request.searchKey) ||
                    a.Task.AssignedBy.FullName.Contains(request.searchKey));
            }

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var assignments = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtoList = assignments.Select(a => new TodayCommentTaskDto
            {
                TaskId = a.TaskId,
                Title = a.Task.Title,
                Status = a.Task.Status,
                StatusText = a.Task.Status.ToString(),
                DueDate = a.Task.DueDate,
                AssignedBy = a.Task.AssignedBy?.FullName,
                Employees = a.Task.Assignments
                    .Where(x => x.IsActive)
                    .Select(x => x.Employee.FullName)
                    .ToList()
            }).ToList();

            var response = new PagedResponse<TodayCommentTaskDto>(
                dtoList,
                totalCount,
                request.PageIndex,
                request.PageSize);

            return ApiResponse<PagedResponse<TodayCommentTaskDto>>.Ok(response);

        }



        public async Task<ApiResponse<List<WarningDto>>> GetWarningsAsync(int employeeId, PeriodDto? period = null)
        {
            var range = PeriodHelper.GetRange(period);

            var warnings = await _warningRepo
                .GetAll(w =>
                    w.TaskAssignment.EmployeeId == employeeId &&
                    w.IssuedAt >= range.Start && w.IssuedAt <= range.End)
                .Select(w => new WarningDto
                {
                    Id = w.Id,
                    Reason = w.Reason,
                    CreatedDate = w.CreatedDate,
                    TaskTitle = w.TaskAssignment.Task.Title,
                    TaskStatus = w.TaskAssignment.Task.Status.ToString()
                })
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();

            return ApiResponse<List<WarningDto>>.Ok(warnings);
        }

        public async Task<ApiResponse<List<DeductionDto>>> GetDeductionsAsync(int employeeId, PeriodDto? period = null)
        {
            var range = PeriodHelper.GetRange(period);

            var deductions = await _deductionRepo
                .GetAll(d =>
                    d.EmployeeId == employeeId &&
                    d.CreatedDate >= range.Start && d.CreatedDate <= range.End)
                .Select(d => new DeductionDto
                {
                    Id = d.Id,
                    Amount = d.Amount,
                    Reason = d.Reason,
                    TaskTitle = d.Task.Title,
                    CreatedDate = d.CreatedDate
                })
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            return ApiResponse<List<DeductionDto>>.Ok(deductions);
        }


        public async Task<ApiResponse<List<MyTaskDto>>> GetDueSoonTasksAsync(int employeeId)
        {
            var now = DateTime.UtcNow;
            var dueSoonDate = now.AddDays(2); 

            var dueSoonTasks = await _assignmentRepo
                .GetAll(a =>
                    a.EmployeeId == employeeId &&
                    a.IsActive &&
                    !a.IsClosed &&
                    a.Task.DueDate != null &&
                    a.Task.DueDate <= dueSoonDate
                )
                .Select(a => new MyTaskDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    Status = a.Task.Status,
                    DueDate = a.Task.DueDate,
                    ProgressPercent = a.ProgressPercent
                })
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            return ApiResponse<List<MyTaskDto>>.Ok(dueSoonTasks);
        }

        public async Task<ApiResponse<EmployeeDashboardKpisExtendedDto>> GetEmployeeKpisAsync(int employeeId, PeriodDto? period = null)
        {
            var range = PeriodHelper.GetRange(period);

            var closedTasksQuery = _assignmentRepo.GetAll(a =>
                a.EmployeeId == employeeId &&
                a.IsClosed &&
                a.Task.ClosedAt != null &&
                a.Task.ClosedAt >= range.Start &&
                a.Task.ClosedAt <= range.End);

            double averageCompletionHours = 0;

            if (await closedTasksQuery.AnyAsync())
            {
                averageCompletionHours = await closedTasksQuery
                    .Select(a => EF.Functions.DateDiffHour(a.Task.CreatedDate, a.Task.ClosedAt!.Value))
                    .AverageAsync();
            }

            var totalClosedTasks = await closedTasksQuery.CountAsync();

            var onTimeTasks = await closedTasksQuery
                .Where(a => a.Task.CloseReason == CloseReason.Manual || a.Task.CloseReason == CloseReason.Admin)
                .CountAsync();

            var onTimeRatePercent = totalClosedTasks == 0 ? 0 : (onTimeTasks * 100.0) / totalClosedTasks;

            var dto = new EmployeeDashboardKpisExtendedDto
            {
                AverageCompletionHours = Math.Round(averageCompletionHours, 2),
                OnTimeRatePercent = Math.Round(onTimeRatePercent, 2)
            };

            return ApiResponse<EmployeeDashboardKpisExtendedDto>.Ok(dto);
        }

        public async Task<ApiResponse<List<CompletedTaskDetailDto>>> GetEmployeeCompletedTasksDetailsAsync(int employeeId, PeriodDto? period = null)
        {
            var range = PeriodHelper.GetRange(period);

            var tasks = await _assignmentRepo
                .GetAll(a =>
                    a.EmployeeId == employeeId &&
                    a.IsClosed &&
                    a.Task.ClosedAt != null &&
                    a.Task.ClosedAt >= range.Start &&
                    a.Task.ClosedAt <= range.End)
                .Select(a => new CompletedTaskDetailDto
                {
                    TaskTitle = a.Task.Title,
                    EmployeeNames = a.Task.Assignments
                                        .Where(x => x.IsActive)
                                        .Select(x => x.Employee.FullName)
                                        .ToList(),
                    CreatedAt = a.Task.CreatedDate,
                    ClosedAt = a.Task.ClosedAt.Value,
                    DurationHours = EF.Functions.DateDiffHour(a.Task.CreatedDate, a.Task.ClosedAt.Value)
                })
                .OrderByDescending(t => t.ClosedAt)
                .ToListAsync();

            return ApiResponse<List<CompletedTaskDetailDto>>.Ok(tasks);
        }


    }
}

