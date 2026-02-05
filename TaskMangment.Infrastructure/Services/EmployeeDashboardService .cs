using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.DiscountTypes;
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
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Discount> _deductionRepo;
        private readonly IRepository<TaskPercentage> _taskPercentageRepo;
        private readonly IStringLocalizer<DiscountAutoType> _localizer;

        private readonly ICachingService _cache;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;
        private readonly IRepository<Employee> _employeeRepo;


        public EmployeeDashboardService(
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<Warning> warningRepo,
            IRepository<Discount> deductionRepo,
            ICachingService cache,
            IRepository<TaskPercentage> taskPercentageRepo,
            IStringLocalizer<DiscountAutoType> localizer,
            IRepository<ManagerBranches> managerBranchesRepo,
            IRepository<Employee> employeeRepo)
        {
            _assignmentRepo = assignmentRepo;
            _warningRepo = warningRepo;
            _deductionRepo = deductionRepo;
            _cache = cache;
            _taskPercentageRepo = taskPercentageRepo;
            _localizer = localizer;
            _managerBranchesRepo = managerBranchesRepo;
            _employeeRepo = employeeRepo;
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



            var assignments = await _assignmentRepo
      .GetAll(a => a.EmployeeId == employeeId && a.IsActive && !a.IsClosed)
      .Include(a => a.Task) 
      .OrderBy(a => a.Task.DueDate)
      .Take(10)
      .ToListAsync();

            var taskIds = assignments.Select(a => a.TaskId).ToList();

            var progressDict = await _taskPercentageRepo
                .GetAll(p => taskIds.Contains(p.TaskId))
                .GroupBy(p => p.TaskId)
                .Select(g => new { TaskId = g.Key, Progress = g.OrderByDescending(x => x.CreatedDate).FirstOrDefault().AchievementPercent })
                .ToDictionaryAsync(x => x.TaskId, x => x.Progress);

            var myTasks = assignments.Select(a => new MyTaskDto
            {
                TaskId = a.TaskId,
                Title = a.Task.Title,
                Status = a.Task.Status,
                DueDate = a.Task.DueDate,
                ProgressPercent = progressDict.ContainsKey(a.TaskId) ? (progressDict[a.TaskId]?.ToString() ?? "") : "0"
            })
            .OrderBy(t => t.DueDate)
            .ToList();

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

        public async Task<ApiResponse<PagedResponse<TodayCommentTaskDto>>> GetTasksWithoutCommentsTodayAsync(
    BaseApiRequest request,
    int employeeId,
    int roleLevel)
        {
            var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Arab Standard Time").Date;
            var allowedStatuses = new[] { WorkTaskStatus.New, WorkTaskStatus.InProgress };

            var baseQuery = _assignmentRepo.GetAll(a =>
                a.IsActive &&
                !a.IsClosed &&
                allowedStatuses.Contains(a.Task.Status) &&
                !a.Task.Comments.Any(c => c.CreatedDate >= today && c.EmployeeId == a.EmployeeId)
            );

            // ===================== Scope حسب RoleLevel =====================
            if (roleLevel < 70)
            {
                baseQuery = baseQuery.Where(a => a.EmployeeId == employeeId);
            }
            else if (roleLevel == 70)
            {
                var myBranchId = await _employeeRepo
                    .GetAll(e => e.Id == employeeId)
                    .Select(e => e.BranchId)
                    .FirstOrDefaultAsync();

                if (!myBranchId.HasValue)
                    baseQuery = baseQuery.Where(a => false);
                else
                    baseQuery = baseQuery.Where(a => a.Employee.BranchId == myBranchId.Value);
            }
            else if (roleLevel == 80)
            {
                var managedBranchIds = await _managerBranchesRepo
                    .GetAll(x => x.ManagerId == employeeId && !x.IsDeleted)
                    .Select(x => x.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!managedBranchIds.Any())
                {
                    baseQuery = baseQuery.Where(a => false);
                }
                else
                {
                    baseQuery = baseQuery.Where(a =>
                        a.Employee.BranchId.HasValue &&
                        managedBranchIds.Contains(a.Employee.BranchId.Value));
                }
            }
            else
            {
                // Admin وما فوق: لا فلترة إضافية
            }

            // ✅ (1) فلترة: لازم أكون طرف في المهمة
            baseQuery = baseQuery.Where(a =>
                a.EmployeeId == employeeId || a.Task.AssignedByEmployeeId == employeeId);

            // ===================== Search =====================
            if (!string.IsNullOrWhiteSpace(request.searchKey))
            {
                var key = request.searchKey.Trim();
                baseQuery = baseQuery.Where(a =>
                    a.Task.Title.Contains(key) ||
                    a.Task.AssignedBy.FullName.Contains(key));
            }

            // ===================== Projection =====================
            var dtoQuery = baseQuery
                .GroupBy(a => new
                {
                    a.TaskId,
                    a.Task.Title,
                    a.Task.Status,
                    a.Task.DueDate,
                    AssignedBy = a.Task.AssignedBy.FullName,
                    a.Task.AssignedByEmployeeId
                })
                .Select(g => new TodayCommentTaskDto
                {
                    TaskId = g.Key.TaskId,
                    Title = g.Key.Title,
                    Status = g.Key.Status,
                    StatusText = g.Key.Status.ToString(),
                    DueDate = g.Key.DueDate,
                    AssignedBy = g.Key.AssignedBy,
                    Employees = g.Select(x => x.Employee.FullName).Distinct().ToList(),

                    RemainDays = g.Key.DueDate.HasValue
                        ? EF.Functions.DateDiffDay(today, g.Key.DueDate.Value)
                        : 0
                })
                .AsNoTracking();

            var totalCount = await dtoQuery.CountAsync();

            dtoQuery = dtoQuery.OrderBy(x => x.DueDate);

            var list = await dtoQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return ApiResponse<PagedResponse<TodayCommentTaskDto>>.Ok(
                new PagedResponse<TodayCommentTaskDto>(list, totalCount, request.PageIndex, request.PageSize)
            );
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
                    CreatedDate = d.CreatedDate,
                    AutoDiscount = d.AutoDiscount,
                    DiscountType = d.discountType

                })
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            foreach (var item in deductions)
            {
                if (!item.AutoDiscount)
                    continue;

                item.Reason = GetAutoDiscountReason(item.DiscountType);
            }


            return ApiResponse<List<DeductionDto>>.Ok(deductions);
        }


        public async Task<ApiResponse<List<MyTaskDto>>> GetDueSoonTasksAsync(int employeeId)
        {
            var now = DateTime.UtcNow;
            var dueSoonDate = now.AddDays(2);

            // Step 1: get due soon assignments
            var assignments = await _assignmentRepo
                .GetAll(a =>
                    a.EmployeeId == employeeId &&
                    a.IsActive &&
                    !a.IsClosed &&
                    a.Task.DueDate != null &&
                    a.Task.DueDate <= dueSoonDate
                )
                .Include(a => a.Task) // make sure Task is loaded
                .OrderBy(a => a.Task.DueDate)
                .ToListAsync();

            // Step 2: fetch progress percentages for these tasks
            var taskIds = assignments.Select(a => a.TaskId).ToList();

            var progressDict = await _taskPercentageRepo
                .GetAll(p => taskIds.Contains(p.TaskId))
                .GroupBy(p => p.TaskId)
                .Select(g => new
                {
                    TaskId = g.Key,
                    Progress = g.OrderByDescending(x => x.CreatedDate)
                                .FirstOrDefault().AchievementPercent
                })
                .ToDictionaryAsync(x => x.TaskId, x => x.Progress);

            // Step 3: map to DTO
            var dueSoonTasks = assignments.Select(a => new MyTaskDto
            {
                TaskId = a.TaskId,
                Title = a.Task.Title,
                Status = a.Task.Status,
                DueDate = a.Task.DueDate,
                ProgressPercent = progressDict.ContainsKey(a.TaskId) ? (progressDict[a.TaskId]?.ToString() ?? "") : "0"
            })
            .OrderBy(a => a.DueDate)
            .ToList();

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

    }
}

