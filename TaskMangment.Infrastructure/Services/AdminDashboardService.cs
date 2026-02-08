using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using TaskMangment.Application.Common.DiscountTypes;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Helpers;
using TaskMangment.Utilities.Localization.Resources;

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
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<ManagerBranches> _managerBranchesRepo;

        private readonly ICachingService _cache;
        private readonly IStringLocalizer<DiscountAutoType> _localizer;
        private readonly IBlobStorageService _blobStorageService;


        public AdminDashboardService(
            IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IRepository<Discount> deductionRepo,
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<Warning> warningRepo,
            IRepository<Attachment> attachmentRepo,
            ICachingService cache,
            IStringLocalizer<DiscountAutoType> localizer,
            IRepository<Branch> branchRepo,
            IRepository<ManagerBranches> managerBranchesRepo,
            IBlobStorageService blobStorageService

        )
        {
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _deductionRepo = deductionRepo;
            _assignmentRepo = assignmentRepo;
            _warningRepo = warningRepo;
            _attachmentRepo = attachmentRepo;
            _cache = cache;
            _localizer = localizer;
            _branchRepo = branchRepo;
            _managerBranchesRepo = managerBranchesRepo;
            _blobStorageService = blobStorageService;
        }

        private async Task<(int? SingleBranchId, List<int>? BranchIds)> GetBranchScopeAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            int? requestedBranchId)
        {
            if (roleLevel >= 100)
            {
                return (requestedBranchId, null);
            }

            if (!employeeId.HasValue)
            {
                return (requestedBranchId, null);
            }

            if (roleLevel == 70)
            {
                var empBranchId = await _employeeRepo
                    .GetAll(e => e.Id == employeeId.Value && e.CompanyId == companyId)
                    .Select(e => e.BranchId)
                    .FirstOrDefaultAsync();

                if (!empBranchId.HasValue)
                    return (null, new List<int>()); 

                return (empBranchId.Value, null);
            }

            if (roleLevel == 80)
            {
                var managedBranchIds = await _managerBranchesRepo
                    .GetAll(x => x.ManagerId == employeeId.Value && !x.IsDeleted)
                    .Join(_branchRepo.GetAll(b => b.CompanyId == companyId && !b.IsDeleted),
                          mb => mb.BranchId,
                          b => b.Id,
                          (mb, b) => mb.BranchId)
                    .Distinct()
                    .ToListAsync();

                if (!managedBranchIds.Any())
                    return (null, new List<int>()); 

                if (requestedBranchId.HasValue && managedBranchIds.Contains(requestedBranchId.Value))
                {
                    return (requestedBranchId.Value, null);
                }

                if (requestedBranchId.HasValue && !managedBranchIds.Contains(requestedBranchId.Value))
                {
                    return (managedBranchIds[0], null);
                }

                return (null, managedBranchIds);
            }

            var myBranch = await _employeeRepo
                .GetAll(e => e.Id == employeeId.Value && e.CompanyId == companyId)
                .Select(e => e.BranchId)
                .FirstOrDefaultAsync();

            if (!myBranch.HasValue)
                return (null, new List<int>());

            return (myBranch.Value, null);
        }

        public async Task<ApiResponse<List<BranchFilterDto>>> GetBranchesForFilterAsync(int companyId, int roleLevel, int employeeId)
        {
            if (roleLevel >= 100)
            {
                var branches = await _branchRepo
                    .GetAll(b => b.CompanyId == companyId && !b.IsDeleted)
                    .Select(b => new BranchFilterDto { Id = b.Id, Name = b.Name })
                    .OrderBy(b => b.Name)
                    .ToListAsync();

                return ApiResponse<List<BranchFilterDto>>.Ok(branches);
            }

            if (roleLevel == 80)
            {
                var ids = await _managerBranchesRepo
                    .GetAll(x => x.ManagerId == employeeId && !x.IsDeleted)
                    .Join(_branchRepo.GetAll(b => b.CompanyId == companyId && !b.IsDeleted),
                          mb => mb.BranchId,
                          b => b.Id,
                          (mb, b) => new BranchFilterDto { Id = b.Id, Name = b.Name })
                    .OrderBy(x => x.Name)
                    .ToListAsync();

                return ApiResponse<List<BranchFilterDto>>.Ok(ids);
            }

            var myBranch = await _employeeRepo
                .GetAll(e => e.Id == employeeId && e.CompanyId == companyId)
                .Select(e => new { e.BranchId, BranchName = e.Branch.Name })
                .FirstOrDefaultAsync();

            if (myBranch?.BranchId == null)
                return ApiResponse<List<BranchFilterDto>>.Ok(new List<BranchFilterDto>());

            return ApiResponse<List<BranchFilterDto>>.Ok(new List<BranchFilterDto>
            {
                new BranchFilterDto { Id = myBranch.BranchId.Value, Name = myBranch.BranchName }
            });
        }

        public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(
            int companyId,
            int roleLevel,
            int? employeeId = null,
            PeriodDto? period = null,
            int? branchId = null)
        {
            string cacheKey = $"dashboard:admin:{companyId}:{roleLevel}:{employeeId}:{branchId}:{period?.Type}";
            var cached = await _cache.GetAsync<AdminDashboardDto>(cacheKey);
            if (cached != null)
                return ApiResponse<AdminDashboardDto>.Ok(cached);

            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var totalEmployeesQuery = _employeeRepo.GetAll(e => e.CompanyId == companyId && e.IsActive);

            if (effectiveBranchId.HasValue)
                totalEmployeesQuery = totalEmployeesQuery.Where(e => e.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                totalEmployeesQuery = totalEmployeesQuery.Where(e => e.BranchId.HasValue && effectiveBranchIds.Contains(e.BranchId.Value));

            var totalEmployees = await totalEmployeesQuery.CountAsync();

            var activeTasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

            if (effectiveBranchId.HasValue)
                activeTasksQuery = activeTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                activeTasksQuery = activeTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

            var activeTasks = await activeTasksQuery.CountAsync();

            var overdueTasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.Status != WorkTaskStatus.Closed &&
                t.DueDate != null &&
                t.DueDate < DateTime.Today &&
                t.DueDate >= range.Start && t.DueDate <= range.End);

            if (effectiveBranchId.HasValue)
                overdueTasksQuery = overdueTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                overdueTasksQuery = overdueTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

            var overdueTasks = await overdueTasksQuery.CountAsync();

            var completedTasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.Status == WorkTaskStatus.Closed &&
                t.ClosedAt >= range.Start && t.ClosedAt <= range.End);

            if (effectiveBranchId.HasValue)
                completedTasksQuery = completedTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                completedTasksQuery = completedTasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

            var completedTasks = await completedTasksQuery.CountAsync();

            var penaltiesQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.CreatedDate >= range.Start && d.CreatedDate <= range.End);

            if (effectiveBranchId.HasValue)
                penaltiesQuery = penaltiesQuery.Where(d => d.Employee.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                penaltiesQuery = penaltiesQuery.Where(d => d.Employee.BranchId.HasValue && effectiveBranchIds.Contains(d.Employee.BranchId.Value));

            var penaltiesThisPeriod = await penaltiesQuery.SumAsync(d => d.Amount);

            var warningsQuery = _warningRepo.GetAll(w =>
                w.Task.CompanyId == companyId &&
                w.IssuedAt >= range.Start && w.IssuedAt <= range.End);

            if (effectiveBranchId.HasValue)
                warningsQuery = warningsQuery.Where(w =>
                    w.Task.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                warningsQuery = warningsQuery.Where(w =>
                    w.Task.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

            var warningsThisPeriod = await warningsQuery.CountAsync();

            var topDelayedQuery = _assignmentRepo.GetAll(a =>
                a.Task.CompanyId == companyId &&
                a.IsActive &&
                a.Task.DueDate >= range.Start && a.Task.DueDate <= range.End &&
                a.Task.Status != WorkTaskStatus.Closed);

            if (effectiveBranchId.HasValue)
                topDelayedQuery = topDelayedQuery.Where(a => a.Employee.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                topDelayedQuery = topDelayedQuery.Where(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value));

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

        public async Task<ApiResponse<PagedResponse<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            UpdatedTodayTasksRequest request,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(request.Period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                (
                    t.Comments.Any(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End) ||
                    t.Assignments.Any(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                ));

            if (effectiveBranchId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            }
            else if (effectiveBranchIds != null)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));
            }

            var dtoQuery = tasksQuery
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
                               : x.LastAssignmentUpdate!.ModifiedDate,
                    UpdatedBy = x.LastComment != null &&
                               (x.LastAssignmentUpdate == null || x.LastComment.CreatedDate >= x.LastAssignmentUpdate.ModifiedDate)
                               ? x.LastComment.EmployeeName
                               : x.LastAssignmentUpdate!.EmployeeName
                })
                .AsNoTracking();

            var totalCount = await dtoQuery.CountAsync();
            dtoQuery = dtoQuery.OrderByDescending(x => x.UpdatedAt);

            var list = await dtoQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return ApiResponse<PagedResponse<UpdatedTodayTaskDto>>.Ok(
                new PagedResponse<UpdatedTodayTaskDto>(list, totalCount, request.PageIndex, request.PageSize)
            );
        }

        public async Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var query = _assignmentRepo.GetAll(a =>
                a.Task.CompanyId == companyId &&
                a.Task.Status == WorkTaskStatus.Closed &&
                a.ModifiedDate >= range.Start &&
                a.ModifiedDate <= range.End);

            if (effectiveBranchId.HasValue)
                query = query.Where(a => a.Employee.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                query = query.Where(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value));



            var attachmentsQuery = _attachmentRepo.GetAll(); 

            var result = await query
                .GroupBy(a => new { a.EmployeeId, a.Employee.FullName })
                .Select(g => new CompletedTodayEmployeeDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeName = g.Key.FullName,
                    CompletedTasksCount = g.Count(),

                    EmployeeImageUrl = attachmentsQuery
                        .Where(att =>
                            att.ReferenceId == g.Key.EmployeeId &&
                            att.AttachmentType == AttachmentType.Employee &&
                            !att.IsDeleted)
                        .Select(att => att.FilePath)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.CompletedTasksCount)
                .ToListAsync();

            foreach (var item in result)
            {
                item.EmployeeImageUrl = _blobStorageService.WithSas(item.EmployeeImageUrl);
            }


            return ApiResponse<List<CompletedTodayEmployeeDto>>.Ok(result);
        }

        public async Task<ApiResponse<List<PendingCloseRequestTaskDto>>> GetPendingCloseRequestsAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var query = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.CloseRequests.Any(r =>
                    r.Status == CloseRequestStatus.Pending &&
                    r.CreatedDate >= range.Start &&
                    r.CreatedDate <= range.End));

            if (effectiveBranchId.HasValue)
            {
                query = query.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            }
            else if (effectiveBranchIds != null)
            {
                query = query.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));
            }

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
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                (
                    (status == "Active" &&
                        (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress))
                    ||
                    (status == "Overdue" &&
                        t.Status != WorkTaskStatus.Closed &&
                        t.DueDate != null &&
                        t.DueDate < DateTime.Today &&
                        t.DueDate >= range.Start && t.DueDate <= range.End)
                    ||
                    (status == "Completed" &&
                        t.Status == WorkTaskStatus.Closed &&
                        t.ClosedAt >= range.Start && t.ClosedAt <= range.End)
                ));

            if (effectiveBranchId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            }
            else if (effectiveBranchIds != null)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));
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

        public async Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var closedTasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.ClosedAt != null &&
                t.ClosedAt >= range.Start &&
                t.ClosedAt <= range.End);

            if (effectiveBranchId.HasValue)
                closedTasksQuery = closedTasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                closedTasksQuery = closedTasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

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

            var highPriorityQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.Priority == TaskPriority.High &&
                (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

            if (effectiveBranchId.HasValue)
                highPriorityQuery = highPriorityQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                highPriorityQuery = highPriorityQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

            var highPriorityOpenTasks = await highPriorityQuery.CountAsync();

            var penaltiesQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.CreatedDate >= range.Start &&
                d.CreatedDate <= range.End);

            if (effectiveBranchId.HasValue)
                penaltiesQuery = penaltiesQuery.Where(d => d.Employee.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                penaltiesQuery = penaltiesQuery.Where(d => d.Employee.BranchId.HasValue && effectiveBranchIds.Contains(d.Employee.BranchId.Value));

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
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var discountsQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.CreatedDate >= range.Start &&
                d.CreatedDate <= range.End &&
                d.Amount > 0);

            if (effectiveBranchId.HasValue)
                discountsQuery = discountsQuery.Where(d => d.Employee.BranchId == effectiveBranchId.Value);
            else if (effectiveBranchIds != null)
                discountsQuery = discountsQuery.Where(d => d.Employee.BranchId.HasValue && effectiveBranchIds.Contains(d.Employee.BranchId.Value));

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
                if (!item.AutoDiscount) continue;
                item.Reason = GetAutoDiscountReason(item.DiscountType);
            }

            return ApiResponse<List<DiscountGetDto>>.Ok(discounts);
        }

        private string GetAutoDiscountReason(DiscountType type)
        {
            return type switch
            {
                DiscountType.AutoCloseTaskDiscount => _localizer[DiscountTypes.AutoCloseTaskDiscount],
                DiscountType.MaxWarningDiscount => _localizer[DiscountTypes.MaxwarningTaskDiscount],
                DiscountType.StopCommentDiscount => _localizer[DiscountTypes.StopCommentTaskDiscount],
                _ => string.Empty
            };
        }

        public async Task<ApiResponse<List<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(
            int companyId,
            int roleLevel,
            int? employeeId,
            int? branchId = null)
        {
            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.Priority == TaskPriority.High &&
                (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

            if (effectiveBranchId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                tasksQuery = tasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

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
            int roleLevel,
            int? employeeId,
            PeriodDto period,
            int? branchId = null)
        {
            var range = PeriodHelper.GetRange(period);

            var scope = await GetBranchScopeAsync(companyId, roleLevel, employeeId, branchId);
            int? effectiveBranchId = scope.SingleBranchId;
            List<int>? effectiveBranchIds = scope.BranchIds;

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.ClosedAt != null &&
                t.ClosedAt >= range.Start &&
                t.ClosedAt <= range.End);

            if (effectiveBranchId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId == effectiveBranchId.Value));
            else if (effectiveBranchIds != null)
                tasksQuery = tasksQuery.Where(t => t.Assignments.Any(a => a.Employee.BranchId.HasValue && effectiveBranchIds.Contains(a.Employee.BranchId.Value)));

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
