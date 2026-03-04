using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using TaskMangment.Application.Common.DiscountTypes;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Dashboards.Admin;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Caching;
using TaskMangment.Infrastructure.Helpers;
using TaskMangment.Infrastructure.Persistence.Extensions;
using TaskMangment.Utilities.Localization.Resources;

namespace TaskMangment.Infrastructure.Services
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

        private readonly IUserAccessContextProvider _accessProvider;

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
            IBlobStorageService blobStorageService,
            IUserAccessContextProvider accessProvider
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
            _accessProvider = accessProvider;
        }

        private async Task<int> GetVersionAsync(string versionKey)
        {
            var v = await _cache.GetAsync<int>(versionKey);
            if (v <= 0)
            {
                await _cache.SetAsync(versionKey, 1, TimeSpan.FromDays(30));
                return 1;
            }
            return v;
        }

        private async Task<(IQueryable<Employee> ScopedEmployeesQuery, IQueryable<int> ScopedEmployeeIds)>
            GetScopedEmployeesAsync(int companyId,int roleLevel,int? employeeId,int? branchId = null)
        {
            IQueryable<Employee> scopedEmployeesQuery = _employeeRepo
                .GetAll(e => e.CompanyId == companyId && e.IsActive);

            if (employeeId.HasValue)
            {
                var access = await _accessProvider.GetAsync(employeeId.Value);
                scopedEmployeesQuery = scopedEmployeesQuery.ApplyAccessScope(access);
            }

            if (roleLevel != 100)
                scopedEmployeesQuery = scopedEmployeesQuery.ApplyRoleHierarchy(roleLevel);

            if (branchId.HasValue)
                scopedEmployeesQuery = scopedEmployeesQuery.Where(e => e.BranchId == branchId.Value);

            var scopedEmployeeIds = scopedEmployeesQuery.Select(e => e.Id);

            return (scopedEmployeesQuery, scopedEmployeeIds);
        }

        public async Task<ApiResponse<List<BranchFilterDto>>> GetBranchesForFilterAsync(int companyId,int roleLevel,int employeeId)
        {
            var access = await _accessProvider.GetAsync(employeeId);

            var branchesQuery = _branchRepo
                .GetAll(b => b.CompanyId == companyId && !b.IsDeleted);

            //var employeesQuery = _employeeRepo
            //    .GetAll(e => e.CompanyId == companyId);

            branchesQuery = branchesQuery.ApplyAccessScope(access);

            if (!access.BranchIds.Any() && !access.FunctionCodes.Any() && roleLevel != 100)
            {
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

            var branches = await branchesQuery
                .Select(b => new BranchFilterDto { Id = b.Id, Name = b.Name })
                .OrderBy(b => b.Name)
                .ToListAsync();

            return ApiResponse<List<BranchFilterDto>>.Ok(branches);
        }

        public async Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId, int roleLevel,int? employeeId = null,PeriodDto? period = null,int? branchId = null)
        {
            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));

            var range = PeriodHelper.GetRange(period);
            var periodKey = period == null
                    ? "all"
                    : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.AdminDashboard(companyId, roleLevel, employeeId, branchId, periodKey, version);

            var dto = await _cache.GetOrSetAsync(
                cacheKey,
                async () =>
                {

                    var (scopedEmployeesQuery, scopedEmployeeIds) =
                        await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

                    var totalEmployees = await scopedEmployeesQuery.CountAsync();

                    var activeTasksQuery = _taskRepo.GetAll(t =>
                        t.CompanyId == companyId &&
                        (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

                    if (employeeId.HasValue || branchId.HasValue)
                    {
                        activeTasksQuery = activeTasksQuery.Where(t =>
                            t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
                    }

                    var activeTasks = await activeTasksQuery.CountAsync();

                    var overdueTasksQuery = _taskRepo.GetAll(t =>
                        t.CompanyId == companyId &&
                        t.Status != WorkTaskStatus.Closed &&
                        t.Status != WorkTaskStatus.AutoClose &&
                        t.Status != WorkTaskStatus.Archived &&
                        t.DueDate != null &&
                        t.DueDate <= DateTime.Today &&
                        t.DueDate >= range.Start && t.DueDate <= range.End);

                    if (employeeId.HasValue || branchId.HasValue)
                    {
                        overdueTasksQuery = overdueTasksQuery.Where(t =>
                            t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
                    }

                    var overdueTasks = await overdueTasksQuery.CountAsync();

                    var completedTasksQuery = _taskRepo.GetAll(t =>
                        t.CompanyId == companyId &&
                        t.Status == WorkTaskStatus.Closed &&
                        t.ClosedAt >= range.Start && t.ClosedAt <= range.End);

                    if (employeeId.HasValue || branchId.HasValue)
                    {
                        completedTasksQuery = completedTasksQuery.Where(t =>
                            t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
                    }

                    var completedTasks = await completedTasksQuery.CountAsync();

            var newTasksQuery = _taskRepo.GetAll(t =>
            t.CompanyId == companyId && 
            t.Status == WorkTaskStatus.New &&
            t.CreatedDate >= range.Start && t.CreatedDate <= range.End);

            if (employeeId.HasValue || branchId.HasValue)
            {
                newTasksQuery = newTasksQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
            }

            var newTasks = await newTasksQuery.CountAsync();


            var penaltiesQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.ViolationDate >= range.Start 
                && d.ViolationDate <= range.End && d.Amount > 0);


            if (employeeId.HasValue || branchId.HasValue)
                penaltiesQuery = penaltiesQuery.Where(d => scopedEmployeeIds.Contains(d.EmployeeId));

                    var penaltiesThisPeriod = await penaltiesQuery.SumAsync(d => d.Amount);

                    var warningsQuery = _warningRepo.GetAll(w =>
                        w.Task.CompanyId == companyId &&
                        w.IssuedAt >= range.Start && w.IssuedAt <= range.End);

                    if (employeeId.HasValue || branchId.HasValue)
                    {
                        warningsQuery = warningsQuery.Where(w =>
                            w.Task.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
                    }

                    var warningsThisPeriod = await warningsQuery.CountAsync();

                    var topDelayedQuery = _assignmentRepo.GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.IsActive &&
                        a.Task.DueDate >= range.Start && a.Task.DueDate <= range.End &&
                        a.Task.Status != WorkTaskStatus.Closed);

                    if (employeeId.HasValue || branchId.HasValue)
                        topDelayedQuery = topDelayedQuery.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

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

                    return new AdminDashboardDto
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
                },
                TimeSpan.FromMinutes(5)
            );

            return ApiResponse<AdminDashboardDto>.Ok(dto);
        }

        public async Task<ApiResponse<PagedResponse<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId, int roleLevel,int? employeeId,UpdatedTodayTasksRequest request,int? branchId = null)
        {

            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));

            var range = PeriodHelper.GetRange(request.Period);

            var periodKey = request.Period == null
                ? "all"
                : $"{request.Period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.UpdatedTodayTasks(companyId,roleLevel,employeeId,branchId,periodKey,request.PageIndex, request.PageSize, version);

            var cachedResult = await _cache.GetOrSetAsync<PagedResponse<UpdatedTodayTaskDto>>(
                cacheKey,
                async () =>
                {

                    var (_, scopedEmployeeIds) =
                await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                (
                    t.Comments.Any(c => c.CreatedDate >= range.Start && c.CreatedDate <= range.End) ||
                    t.Assignments.Any(a => a.ModifiedDate >= range.Start && a.ModifiedDate <= range.End)
                ));

            if (employeeId.HasValue || branchId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
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

            var list = await dtoQuery
                .OrderByDescending(x => x.UpdatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

                    return new PagedResponse<UpdatedTodayTaskDto>(list, totalCount, request.PageIndex, request.PageSize);
                },
                TimeSpan.FromMinutes(5)
                );

            return ApiResponse<PagedResponse<UpdatedTodayTaskDto>>.Ok(cachedResult);
        }


        public async Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId,int roleLevel,int? employeeId,PeriodDto period, int? branchId = null)
        {
            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));
            var range = PeriodHelper.GetRange(period);

            var periodKey = period == null
                ? "all"
                : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.CompletedTodayEmployees(companyId, roleLevel, employeeId, branchId, periodKey, version);

            var result = await _cache.GetOrSetAsync<List<CompletedTodayEmployeeDto>>(
                cacheKey,
                async () =>
                {
                    var (_, scopedEmployeeIds) =
                        await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

                    var query = _assignmentRepo.GetAll(a =>
                        a.Task.CompanyId == companyId &&
                        a.Task.Status == WorkTaskStatus.Closed &&
                        a.ModifiedDate >= range.Start &&
                        a.ModifiedDate <= range.End);

                    if (employeeId.HasValue || branchId.HasValue)
                        query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

                    var attachmentsQuery = _attachmentRepo.GetAll();

                    var list = await query
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

                    return list;
                },
                TimeSpan.FromSeconds(45)
            );

            foreach (var item in result)
                item.EmployeeImageUrl = _blobStorageService.WithSas(item.EmployeeImageUrl);

            return ApiResponse<List<CompletedTodayEmployeeDto>>.Ok(result);
        }


        public async Task<ApiResponse<List<PendingCloseRequestTaskDto>>> GetPendingCloseRequestsAsync(int companyId,int roleLevel,int? employeeId,PeriodDto period,int? branchId = null)
        {
            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));
            var range = PeriodHelper.GetRange(period);

            var periodKey = period == null
                ? "all"
                : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.PendingCloseRequests(companyId, roleLevel, employeeId, branchId, periodKey, version);

            var result = await _cache.GetOrSetAsync<List<PendingCloseRequestTaskDto>>(
                cacheKey,
                async () =>
                {
                    var (_, scopedEmployeeIds) =
                        await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

                    var query = _taskRepo.GetAll(t =>
                        t.CompanyId == companyId &&
                        t.CloseRequests.Any(r =>
                            r.Status == CloseRequestStatus.Pending &&
                            r.CreatedDate >= range.Start &&
                            r.CreatedDate <= range.End));

                    if (employeeId.HasValue || branchId.HasValue)
                    {
                        query = query.Where(t =>
                            t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
                    }

                    var list = await query
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

                    return list;
                },
                TimeSpan.FromSeconds(45)
            );

            return ApiResponse<List<PendingCloseRequestTaskDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResponse<TaskStatusDto>>> GetTasksByStatusAsync(
    int companyId,
    string status,
    int roleLevel,
    int? employeeId,
    TasksByStatusRequest request,
    int? branchId = null)
        {
            var range = PeriodHelper.GetRange(request.Period);

            var periodKey = period == null
                ? "all"
                : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.TasksByStatus(companyId, status, roleLevel, employeeId, branchId, periodKey, version);

            var tasks = await _cache.GetOrSetAsync<List<TaskStatusDto>>(
                cacheKey,
                async () =>
                {
                    var (_, scopedEmployeeIds) =
                        await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

            var tasksQuery = _taskRepo.GetAll(t => t.CompanyId == companyId);

            switch (status)
            {
                case "Active":
                    tasksQuery = tasksQuery.Where(t =>
                        t.Status == WorkTaskStatus.New ||
                        t.Status == WorkTaskStatus.InProgress);
                    break;

                case "New":
                    tasksQuery = tasksQuery.Where(t =>
                        t.Status == WorkTaskStatus.New &&
                        t.CreatedDate >= range.Start &&
                        t.CreatedDate <= range.End);
                    break;

                case "Overdue":
                    tasksQuery = tasksQuery.Where(t =>
                        t.Status != WorkTaskStatus.Closed &&
                        t.Status != WorkTaskStatus.AutoClose &&
                        t.Status != WorkTaskStatus.Archived &&
                        t.DueDate != null &&
                        t.DueDate <= DateTime.Today &&
                        t.DueDate >= range.Start &&
                        t.DueDate <= range.End);
                    break;

                case "Completed":
                    tasksQuery = tasksQuery.Where(t =>
                        t.Status == WorkTaskStatus.Closed &&
                        t.ClosedAt >= range.Start &&
                        t.ClosedAt <= range.End);
                    break;
            }
            if (employeeId.HasValue || branchId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
            }

            var dtoQuery = tasksQuery
                .Select(t => new TaskStatusDto
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    StatusText = t.Status.ToString(),
                    Employees = t.Assignments
                        .Where(a => a.IsActive)
                        .Select(a => a.Employee.FullName)
                        .ToList()
                })
                .AsNoTracking();

            var totalCount = await dtoQuery.CountAsync();

            var list = await dtoQuery
                 .OrderByDescending(x => x.TaskId)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return ApiResponse<PagedResponse<TaskStatusDto>>.Ok(
                new PagedResponse<TaskStatusDto>(list, totalCount, request.PageIndex, request.PageSize)
            );
        }

        public async Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync( int companyId,int roleLevel, int? employeeId,PeriodDto period,int? branchId = null)
        {
            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));
            var range = PeriodHelper.GetRange(period);

            var periodKey = period == null ? "all" : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.AdminKpisExtended(companyId, roleLevel, employeeId, branchId, periodKey, version);

            var dto = await _cache.GetOrSetAsync(cacheKey, async () =>
            {
                var (_, scopedEmployeeIds) =
                await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

            var closedTasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.ClosedAt != null &&
                t.ClosedAt >= range.Start &&
                t.ClosedAt <= range.End);

            if (employeeId.HasValue || branchId.HasValue)
                closedTasksQuery = closedTasksQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));

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

            if (employeeId.HasValue || branchId.HasValue)
                highPriorityQuery = highPriorityQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));

            var highPriorityOpenTasks = await highPriorityQuery.CountAsync();

            var penaltiesQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.ViolationDate >= range.Start &&
                d.ViolationDate <= range.End);

            if (employeeId.HasValue || branchId.HasValue)
                penaltiesQuery = penaltiesQuery.Where(d => scopedEmployeeIds.Contains(d.EmployeeId));

            var penalties = await penaltiesQuery.SumAsync(d => d.Amount);

            var result = new AdminKpisExtendedDto
            {
                AverageCompletionHours = Math.Round(averageCompletionHours, 2),
                OnTimeRatePercent = Math.Round(onTimeRatePercent, 2),
                HighPriorityOpenTasks = highPriorityOpenTasks,
                PenaltiesThisMonth = penalties
            };
                return result;
            }, TimeSpan.FromMinutes(5));

            return ApiResponse<AdminKpisExtendedDto>.Ok(dto);



        }

        public async Task<ApiResponse<PagedResponse<DiscountGetDto>>> GetDiscountsAsync(
    int companyId,
    int roleLevel,
    int? employeeId,
    DiscountsRequest request,
    int? branchId = null)
        {
            var range = PeriodHelper.GetRange(request.Period);

                var (_, scopedEmployeeIds) =
                await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

            var discountsQuery = _deductionRepo.GetAll(d =>
                d.Employee.CompanyId == companyId &&
                d.ViolationDate >= range.Start &&
                d.ViolationDate <= range.End &&
                d.Amount > 0);

            if (employeeId.HasValue || branchId.HasValue)
                discountsQuery = discountsQuery.Where(d => scopedEmployeeIds.Contains(d.EmployeeId));

            var dtoQuery = discountsQuery
                .Select(d => new DiscountGetDto
                {
                    TaskId = d.TaskId,
                    EmployeeName = d.Employee.FullName,
                    TaskTitle = d.Task.Title,
                    CreatedDate = d.ViolationDate,
                    Reason = d.Reason,
                    Amount = d.Amount,
                    AutoDiscount = d.AutoDiscount,
                    DiscountType = d.discountType,
                    ViolationDate = d.ViolationDate
                })
                .AsNoTracking();

            var totalCount = await dtoQuery.CountAsync();

            var list = await dtoQuery
                .OrderByDescending(d => d.CreatedDate)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            foreach (var item in list)
            {
                if (!item.AutoDiscount) continue;
                item.Reason = GetAutoDiscountReason(item.DiscountType);
            }
                return discounts;
            }, TimeSpan.FromMinutes(5));

            return ApiResponse<PagedResponse<DiscountGetDto>>.Ok(
                new PagedResponse<DiscountGetDto>(list, totalCount, request.PageIndex, request.PageSize)
            );
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

        public async Task<ApiResponse<PagedResponse<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(
    int companyId,
    int roleLevel,
    int? employeeId,
    TasksHighPriorityRequest request, 
    int? branchId = null)
        {
            var (_, scopedEmployeeIds) =
                await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

            var tasksQuery = _taskRepo.GetAll(t =>
                t.CompanyId == companyId &&
                t.Priority == TaskPriority.High &&
                (t.Status == WorkTaskStatus.New || t.Status == WorkTaskStatus.InProgress));

            if (employeeId.HasValue || branchId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t =>
                    t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));
            }

            var dtoQuery = tasksQuery
                .Select(t => new HighPriorityTaskDto
                {
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    Employees = t.Assignments
                        .Where(a => a.IsActive)
                        .Select(a => a.Employee.FullName)
                        .ToList(),
                    Status = t.Status,
                    StatusText = t.Status.ToString(),
                    CreateDate= t.CreatedDate,
                    DueDate = t.DueDate
                })
                .AsNoTracking();

            var totalCount = await dtoQuery.CountAsync();

            var list = await dtoQuery
                .OrderByDescending(x => x.TaskId)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            }, TimeSpan.FromMinutes(2));

            return ApiResponse<PagedResponse<HighPriorityTaskDto>>.Ok(
                new PagedResponse<HighPriorityTaskDto>(list, totalCount, request.PageIndex, request.PageSize)
            );
        }

        public async Task<ApiResponse<List<CompletedTaskDetailDto>>> GetCompletedTasksDetailsAsync(int companyId,int roleLevel,int? employeeId, PeriodDto period,int? branchId = null)
        {
            var version = await GetVersionAsync(CacheKeys.DashboardVersion(companyId));

            var range = PeriodHelper.GetRange(period);
            var periodKey = period == null
                ? "all"
                : $"{period.Type}:{range.Start:yyyyMMdd}:{range.End:yyyyMMdd}";

            var cacheKey = CacheKeys.CompletedTasksDetails(companyId, roleLevel, employeeId, branchId, periodKey, version);

            var tasks = await _cache.GetOrSetAsync<List<CompletedTaskDetailDto>>(
                cacheKey,
                async () =>
                {
                    var (_, scopedEmployeeIds) =
                        await GetScopedEmployeesAsync(companyId, roleLevel, employeeId, branchId);

                    var tasksQuery = _taskRepo.GetAll(t =>
                        t.CompanyId == companyId &&
                        t.ClosedAt != null &&
                        t.ClosedAt >= range.Start &&
                        t.ClosedAt <= range.End);

                    if (employeeId.HasValue || branchId.HasValue)
                        tasksQuery = tasksQuery.Where(t =>
                            t.Assignments.Any(a => scopedEmployeeIds.Contains(a.EmployeeId)));

            var tasks = await tasksQuery
                .Select(t => new CompletedTaskDetailDto
                {
                    TaskId = t.Id,
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

                    return list;
                },
                TimeSpan.FromMinutes(2)
            );

            return ApiResponse<List<CompletedTaskDetailDto>>.Ok(tasks);
        }

    }
}
