using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs.ReportsDTO;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IPermissionChecker _permissionChecker;

        public ReportService(AppDbContext context, IUserAccessContextProvider accessProvider, IPermissionChecker permissionChecker)
        {
            _context = context;
            _accessProvider = accessProvider;
            _permissionChecker = permissionChecker;
        }

        /// <summary>
        /// Helper
        /// </summary>
        private async Task<(IQueryable<int> ScopedEmployeeIds, bool CanViewAllTasks, bool CanViewCreatedTasks, bool HasAccessScope)> GetScopedEmployeeIdsAsync(int currentEmployeeId, int roleLevel)
        {
            var access = await _accessProvider.GetAsync(currentEmployeeId);

            var companyId = await _context.Employees
                .Where(e => e.Id == currentEmployeeId)
                .Select(e => e.CompanyId)
                .FirstAsync();

            bool canViewAllTasks = false;

            if (roleLevel >= 100)
            {
                canViewAllTasks =
                    await _permissionChecker
                        .HasPermissionAsync(
                            currentEmployeeId,
                            "VIEW_ALL_TASKS");
            }

            bool canViewCreatedTasks =
                await _permissionChecker
                    .HasPermissionAsync(
                        currentEmployeeId,
                        "CREATE_TASK");

            var hasAccessScope = access.BranchIds.Any() || access.FunctionCodes.Any();

            IQueryable<Employee> scopedEmployeesQuery;

            if (roleLevel >= 100 && canViewAllTasks)
            {
                // يشوف كل الشركة
                scopedEmployeesQuery = _context.Employees
                    .Where(e =>
                        e.CompanyId == companyId &&
                        e.IsActive);
            }
            else if (hasAccessScope)
            {
                // يشوف الـ Access
                scopedEmployeesQuery = _context.Employees
                    .Where(e =>
                        e.CompanyId == companyId &&
                        e.IsActive)
                    .ApplyAccessScope(access);

                if (roleLevel != 100)
                {
                    scopedEmployeesQuery =
                        scopedEmployeesQuery
                            .ApplyRoleHierarchy(roleLevel);
                }
            }
            else if (canViewCreatedTasks)
            {
                // يشوف موظفي الفرع
                var branchId = await _context.Employees
                    .Where(e => e.Id == currentEmployeeId)
                    .Select(e => e.BranchId)
                    .FirstAsync();

                scopedEmployeesQuery = _context.Employees
                    .Where(e =>
                        e.CompanyId == companyId &&
                        e.BranchId == branchId &&
                        e.IsActive);
            }
            else
            {
                // يشوف نفسه فقط
                scopedEmployeesQuery = _context.Employees
                    .Where(e =>
                        e.Id == currentEmployeeId &&
                        e.IsActive);
            }

            var scopedEmployeeIds =
                scopedEmployeesQuery.Select(e => e.Id);

            return (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope);
        }

        private static int? ResolveRoleFilter(int roleLevel, int? roleId)
        {
            // Level 100: any role. Level 70/80: allowed (results stay limited by access/branch scope).
            if (!roleId.HasValue)
                return null;

            return roleLevel >= 70 ? roleId : null;
        }

        private IQueryable<int> ApplyRoleFilter(IQueryable<int> scopedEmployeeIds, int? roleId, int roleLevel)
        {
            if (!roleId.HasValue)
                return scopedEmployeeIds;

            return scopedEmployeeIds.Where(employeeId =>
                _context.EmployeeRoles.Any(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.RoleId == roleId.Value &&
                    er.Role != null &&
                    !er.Role.IsDeleted &&
                    (roleLevel >= 100 || er.Role.Level <= roleLevel)));
        }

        public async Task<List<ReportRoleOptionDto>> GetReportFilterRolesAsync(int currentEmployeeId, int roleLevel)
        {
            if (roleLevel < 70)
                return new List<ReportRoleOptionDto>();

            if (roleLevel == 80)
            {
                var functionCode = await _context.Employees
                    .Where(e => e.Id == currentEmployeeId)
                    .Select(e => e.FunctionCode)
                    .FirstOrDefaultAsync();

                if (functionCode != FunctionCode.Operations)
                    return new List<ReportRoleOptionDto>();
            }

            var (scopedEmployeeIds, _, _, _) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            var rolesQuery = _context.EmployeeRoles
                .Where(er =>
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    scopedEmployeeIds.Contains(er.EmployeeId) &&
                    er.Role != null &&
                    !er.Role.IsDeleted);

            if (roleLevel < 100)
                rolesQuery = rolesQuery.Where(er => er.Role.Level <= roleLevel);

            return await rolesQuery
                .Select(er => new ReportRoleOptionDto
                {
                    Id = er.RoleId,
                    Name = er.Role.Name,
                    Level = er.Role.Level
                })
                .Distinct()
                .OrderBy(r => r.Level)
                .ThenBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<EmployeeCommentsActivityReportDto>> GetEmployeesCommentsActivityAsync(
            int currentEmployeeId,
            int roleLevel,
            DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            var query = _context.TaskComments
                .Include(c => c.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeCommentsActivityReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId.Value));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(a =>
                        a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a =>
                        scopedEmployeeIds.Contains(a.EmployeeId.Value)
                    );
                }
            }

            return await query
                .GroupBy(c => new
                {
                    c.EmployeeId,
                    c.Employee.FullName
                })
                .Select(g => new EmployeeCommentsActivityReportDto
                {
                    EmployeeName = g.Key.FullName,
                    TotalComments = g.Count(),
                    DistinctTasksCount = g.Select(x => x.TaskId).Distinct().Count(),
                    AvgCommentsPerTask = g.Select(x => x.TaskId).Distinct().Count() == 0
                        ? 0
                        : (decimal)g.Count() / g.Select(x => x.TaskId).Distinct().Count(),
                    LastCommentDate = g.Max(x => x.CreatedDate)
                })
                .OrderByDescending(x => x.TotalComments)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EmployeeCommentsReportDto>> GetTopEmployeesByCommentsAsync(
            int currentEmployeeId,
            int roleLevel,
            DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            var query = _context.TaskComments
                .Include(c => c.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeCommentsReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId.Value));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(a =>
                       // scopedEmployeeIds.Contains(a.EmployeeId.Value) ||
                        a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a =>
                        scopedEmployeeIds.Contains(a.EmployeeId.Value)
                    );
                }
            }

            return await query
                .GroupBy(c => new
                {
                    c.EmployeeId,
                    c.Employee.FullName
                })
                .Select(g => new EmployeeCommentsReportDto
                {
                    EmployeeName = g.Key.FullName,
                    CommentsCount = g.Count(),
                    DistinctTasksCount = g.Select(x => x.TaskId).Distinct().Count(),
                    AvgCommentsPerTask = g.Select(x => x.TaskId).Distinct().Count() == 0
    ? 0
    : Math.Round(
        (decimal)g.Count() / g.Select(x => x.TaskId).Distinct().Count(),
        2
      ),
                    LastCommentDate = g.Max(x => x.CreatedDate)
                })
                .OrderByDescending(x => x.CommentsCount)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EmployeeAssignmentsReportDto>> GetMostAssignedEmployeesAsync(
    int currentEmployeeId,
    int roleLevel,
    DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            var query = _context.TaskAssignments
                .Include(a => a.Employee)
                .Include(a => a.Task)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeAssignmentsReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(a =>
                       // scopedEmployeeIds.Contains(a.EmployeeId) ||
                        a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));
                }
            }

            DateTime today = DateTime.Today;
            DateTime closingSoonDate = today.AddDays(3);

            return await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    a.Employee.FullName
                })
                .Select(g => new EmployeeAssignmentsReportDto
                {
                    EmployeeName = g.Key.FullName,
                    TotalTasks = g.Count(),
                    NewTasks = g.Count(x => x.Task.Status == WorkTaskStatus.New),
                    InProgressTasks = g.Count(x => x.Task.Status == WorkTaskStatus.InProgress),
                    ClosedTasks = g.Count(x => x.Task.Status == WorkTaskStatus.Closed),

                    OverdueTasks = g.Count(x =>
                        x.Task.Status != WorkTaskStatus.Closed &&
                        x.Task.DueDate != null &&
                        x.Task.DueDate < today),

                    ClosingSoonTasks = g.Count(x =>
                        x.Task.Status != WorkTaskStatus.Closed &&
                        x.Task.DueDate != null &&
                        x.Task.DueDate >= today &&
                        x.Task.DueDate <= closingSoonDate),

                    CompletionRate = g.Count(x => x.Task.Status == WorkTaskStatus.Closed) == 0
                        ? 0
                        : Math.Round(
                            (decimal)g.Count(x => x.Task.Status == WorkTaskStatus.Closed) * 100 / g.Count(),
                            2)
                })
                .OrderByDescending(x => x.TotalTasks)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<EmployeeOnTimeReportDto>> GetOnTimeCompletionReportAsync(
            int currentEmployeeId,
            int roleLevel,
            DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            var query = _context.TaskAssignments
                .Include(a => a.Employee)
                .Include(a => a.Task)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.ClosedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.ClosedAt <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeOnTimeReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
     await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(a =>a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a =>
                        a.EmployeeId == currentEmployeeId
                    );
                }
            }

            query = query.Where(a =>
                a.Task.Status == WorkTaskStatus.Closed &&
                a.Task.ClosedAt != null &&
                a.Task.DueDate != null);

            var result = await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    EmployeeName = a.Employee.FullName
                })
                .Select(g => new EmployeeOnTimeReportDto
                {
                    EmployeeName = g.Key.EmployeeName,
                    TotalClosedTasks = g.Count(),
                    OnTimeTasks = g.Count(x => x.Task.ClosedAt <= x.Task.DueDate),
                    LateTasks = g.Count(x => x.Task.ClosedAt > x.Task.DueDate),
                    CommitmentPercentage = g.Count() == 0
                        ? 0
                        : (decimal)g.Count(x => x.Task.ClosedAt <= x.Task.DueDate) * 100 / g.Count()
                })
                .Where(x => x.TotalClosedTasks > 0)
                .OrderByDescending(x => x.CommitmentPercentage)
                .ThenByDescending(x => x.TotalClosedTasks)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<EmployeeArchivedTasksReportDto>> GetMostArchivedEmployeesAsync(
            int currentEmployeeId,
            int roleLevel,
            DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            var query = _context.TaskAssignments
                .Include(a => a.Employee)
                .Include(a => a.Task)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Task.CreatedDate <= toDate.Value);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<EmployeeArchivedTasksReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
    await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(a =>
                        //scopedEmployeeIds.Contains(a.EmployeeId) ||
                        a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a =>
                        scopedEmployeeIds.Contains(a.EmployeeId)
                    );
                }
            }

            var result = await query
                .GroupBy(a => new
                {
                    a.EmployeeId,
                    EmployeeName = a.Employee.FullName
                })
                .Select(g => new EmployeeArchivedTasksReportDto
                {
                    EmployeeName = g.Key.EmployeeName,
                    TotalTasks = g.Count(),
                    ArchivedTasksCount = g.Count(x => x.Task.Status == WorkTaskStatus.Archived),
                    ArchiveRate = g.Count() == 0 ? 0
                    : Math.Round((decimal)g.Count(x => x.Task.Status == WorkTaskStatus.Archived) * 100 / g.Count(), 2)

                })
                .Where(x => x.ArchivedTasksCount > 0)
                .OrderByDescending(x => x.ArchiveRate)
                .ThenByDescending(x => x.ArchivedTasksCount)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<TaskDiscountAuditReportDto> GetTaskDiscountAuditReportAsync(
    int currentEmployeeId,
    int roleLevel,
    TaskDiscountReportFilterDto dto)
        {
            IQueryable<Discount> query = _context.Discounts
                .Include(d => d.Task)
                    .ThenInclude(t => t.AssignedBy)
                .Include(d => d.Employee)
                .Where(d => !d.IsDeleted && d.Amount > 0 && !d.Task.IsDeleted)
                .AsQueryable();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
    await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            query = query.Where(d => !d.IsDeleted && d.Amount > 0);

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(d =>
                        d.Task.AssignedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    if (dto.MovementType == TaskMovementType.Incoming)
                    {
                        query = query.Where(d =>
                            scopedEmployeeIds.Contains(d.EmployeeId)
                        );
                    }
                    else
                    {
                        query = query.Where(d =>
                            scopedEmployeeIds.Contains(d.Task.AssignedByEmployeeId.Value)
                        );
                    }
                }
            }

            if (dto.MovementType == TaskMovementType.Incoming)
            {
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
                {
                    if (!canViewAllTasks && !hasAccessScope && dto.EmployeeId.Value != currentEmployeeId)
                        query = query.Where(d => false);
                    else
                        query = query.Where(d => d.EmployeeId == dto.EmployeeId.Value);
                }
            }
            else
            {
                if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
                {
                    if (!canViewAllTasks && !hasAccessScope && dto.EmployeeId.Value != currentEmployeeId)
                        query = query.Where(d => false);
                    else
                        query = query.Where(d => d.Task.AssignedByEmployeeId == dto.EmployeeId.Value);
                }
            }

            if (dto.FromDate.HasValue)
                query = query.Where(d => d.ViolationDate >= dto.FromDate.Value);

            if (dto.ToDate.HasValue)
            {
                var toDateExclusive = dto.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.ViolationDate < toDateExclusive);
            }

            if (dto.Status.HasValue)
                query = query.Where(d => d.Task.Status == dto.Status.Value);


            var flatRows = await query
                .GroupBy(d => new
                {
                    d.TaskId,
                    d.EmployeeId,

                    Title = d.Task.Title,
                    DueDate = d.Task.DueDate,
                    TaskStatus = d.Task.Status,

                    AssignedByName = d.Task.AssignedBy != null ? d.Task.AssignedBy.FullName : "غير معروف",
                    EmployeeName = d.Employee != null ? d.Employee.FullName : "غير معروف"
                })
                .Select(g => new TaskDiscountAuditRowDto
                {
                    TaskId = g.Key.TaskId,
                    Title = g.Key.Title,
                    AssignedBy = g.Key.AssignedByName,
                    ClosedDate = g.Key.DueDate,

                    Status =
                        g.Key.TaskStatus == WorkTaskStatus.Archived ? "مؤرشفة" :
                        g.Key.TaskStatus == WorkTaskStatus.Closed ? "مغلقة" :
                        g.Key.TaskStatus == WorkTaskStatus.AutoClose ? "مغلقة تلقائيًا" :
                        g.Key.TaskStatus == WorkTaskStatus.InProgress ? "قيد التنفيذ" :
                        g.Key.TaskStatus == WorkTaskStatus.New ? "جديدة" :
                        "غير محدد",

                    AutoDiscount = g.Where(x => x.AutoDiscount)
                                    .Sum(x => (decimal?)x.Amount) ?? 0,

                    ManualDiscount = g.Where(x => !x.AutoDiscount)
                                      .Sum(x => (decimal?)x.Amount) ?? 0,

                    EmployeeName = g.Key.EmployeeName,

                    GroupEmployeeName =
                        dto.MovementType == TaskMovementType.Incoming
                            ? g.Key.EmployeeName
                            : g.Key.AssignedByName
                })
                .AsNoTracking()
                .ToListAsync();

            var groups = flatRows
                        .GroupBy(x => new
                        {
                            x.EmployeeName,
                            x.GroupEmployeeName
                        })
                .Select(g => new EmployeeDiscountAuditGroupDto
                {
                    EmployeeName = g.Key.GroupEmployeeName,
                    Tasks = g.OrderBy(x => x.ClosedDate).ToList()
                })
                .OrderByDescending(g => g.TotalDiscount)
                .ToList();

            return new TaskDiscountAuditReportDto
            {
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Groups = groups,
                MovementType = dto.MovementType
            };
        }


        public async Task<List<EmployeeTotalDiscountReportRowDto>> GetEmployeeTotalDiscountReportAsync(
            int currentEmployeeId,
            int roleLevel,
            EmployeeTotalDiscountReportFilterDto dto)
        {
            if (roleLevel < 100)
                return new List<EmployeeTotalDiscountReportRowDto>();

            var currentEmployeeCompanyId = await _context.Employees
                .Where(e => e.Id == currentEmployeeId)
                .Select(e => e.CompanyId)
                .FirstAsync();

            var effectiveFromDate = dto.FromDate?.Date;
            var effectiveToDate = dto.ToDate.HasValue
                ? dto.ToDate.Value.Date.AddDays(1)
                : (dto.FromDate.HasValue ? DateTime.Now : (DateTime?)null);

            var discountsQuery = _context.Discounts
                .Where(d =>
                    !d.IsDeleted &&
                    d.Amount > 0 &&
                    d.Employee != null &&
                    d.Employee.IsActive &&
                    d.Employee.CompanyId == currentEmployeeCompanyId);

            if (dto.BranchId.HasValue && dto.BranchId.Value > 0)
                discountsQuery = discountsQuery.Where(d => d.Employee.BranchId == dto.BranchId.Value);

            var query = discountsQuery
                .SelectMany(
                    d => d.Employee.EmployeeRoles.Where(er =>
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Role != null &&
                        !er.Role.IsDeleted &&
                        (!er.Role.CompanyId.HasValue || er.Role.CompanyId == currentEmployeeCompanyId) &&
                        (!dto.RoleId.HasValue || er.RoleId == dto.RoleId.Value)),
                    (d, er) => new
                    {
                        d.EmployeeId,
                        EmployeeName = d.Employee.FullName,
                        d.Amount,
                        d.ViolationDate,
                        RoleId = er.RoleId,
                        RoleTitle = er.Role.Name
                    });

            if (effectiveFromDate.HasValue)
                query = query.Where(x => x.ViolationDate >= effectiveFromDate.Value);

            if (effectiveToDate.HasValue)
                query = query.Where(x => x.ViolationDate < effectiveToDate.Value);

            return await query
                .GroupBy(x => new
                {
                    x.EmployeeId,
                    x.EmployeeName,
                    x.RoleId,
                    x.RoleTitle
                })
                .Select(g => new EmployeeTotalDiscountReportRowDto
                {
                    EmployeeName = g.Key.EmployeeName ?? "غير معروف",
                    RoleTitle = g.Key.RoleTitle ?? "-",
                    TotalDiscount = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.RoleTitle)
                .ThenByDescending(x => x.TotalDiscount)
                .ThenBy(x => x.EmployeeName)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<TaskDiscountReportDto>> GetTaskDiscountReportAsync(
    int currentEmployeeId,
    int roleLevel,
    TaskDiscountReportFilterDto dto)
        {
            if (!dto.FromDate.HasValue && !dto.ToDate.HasValue)
                return new List<TaskDiscountReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
    await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            IQueryable<Discount> query = _context.Discounts
                .Include(d => d.Task)
                .ThenInclude(t => t.AssignedBy)
                .Include(d => d.Employee)
                .Where(d => !d.IsDeleted && d.Amount > 0 && !d.Task.IsDeleted)
                .AsQueryable();

            query = query.Where(d => scopedEmployeeIds.Contains(d.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(d =>
                        d.Task.AssignedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    if (dto.MovementType == TaskMovementType.Incoming)
                    {
                        query = query.Where(d =>
                            scopedEmployeeIds.Contains(d.EmployeeId)
                        );
                    }
                    else
                    {
                        query = query.Where(d =>
                            scopedEmployeeIds.Contains(d.Task.AssignedByEmployeeId.Value)
                        );
                    }
                }
            }

            if (dto.EmployeeId.HasValue && dto.EmployeeId.Value > 0)
            {
                if (!canViewAllTasks && !hasAccessScope && dto.EmployeeId.Value != currentEmployeeId)
                    return new List<TaskDiscountReportDto>();

                if (dto.MovementType == TaskMovementType.Incoming)
                    query = query.Where(d => d.EmployeeId == dto.EmployeeId.Value);
                else
                    query = query.Where(d => d.Task.AssignedByEmployeeId == dto.EmployeeId.Value);
            }

            if (dto.FromDate.HasValue)
                query = query.Where(d => d.ViolationDate >= dto.FromDate.Value);

            if (dto.ToDate.HasValue)
            {
                var toDateExclusive = dto.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.ViolationDate < toDateExclusive);
            }

            if (dto.Status.HasValue)
                query = query.Where(d => d.Task.Status == dto.Status.Value);

           // query = query.Where(d => !d.IsDeleted && d.Amount > 0);

            var result = await query
                .GroupBy(d => new
                {
                    d.TaskId,
                    d.EmployeeId,
                    TaskTitle = d.Task.Title,
                    AssignedByName = d.Task.AssignedBy.FullName,
                    ClosedDate = d.Task.DueDate,
                    TaskStatus = d.Task.Status,
                    EmployeeName = d.Employee.FullName
                })
                .Select(g => new TaskDiscountReportDto
                {
                    TaskId = g.Key.TaskId,
                    Title = g.Key.TaskTitle,
                    AssignedBy = g.Key.AssignedByName,
                    ClosedDate = g.Key.ClosedDate,

                    Status = g.Key.TaskStatus == WorkTaskStatus.Archived ? "مورشف" :
                             g.Key.TaskStatus == WorkTaskStatus.Closed ? "مغلقة" :
                             g.Key.TaskStatus == WorkTaskStatus.AutoClose ? "مغلق تلقائي" :
                             g.Key.TaskStatus == WorkTaskStatus.InProgress ? "قيد التنفيذ" :
                             g.Key.TaskStatus == WorkTaskStatus.New ? "جديدة" :
                             "غير محدد",

                    AutoDiscount = g.Where(x => x.AutoDiscount)
                                    .Sum(x => (decimal?)x.Amount) ?? 0,

                    ManualDiscount = g.Where(x => !x.AutoDiscount)
                                      .Sum(x => (decimal?)x.Amount) ?? 0,

                    Evaluation = g.Key.TaskStatus == WorkTaskStatus.Archived ? "جيد" :
                                 g.Key.TaskStatus == WorkTaskStatus.Closed ? "جيد" :
                                 g.Key.TaskStatus == WorkTaskStatus.AutoClose ? "سيئ" :
                                 "قيد التنفيذ",

                    EmployeeName = g.Key.EmployeeName
                })
                .OrderBy(r => r.EmployeeName)
                .ThenBy(r => r.ClosedDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        public async Task<List<TaskActivityReportDto>> GetTaskActivityReportAsync(
            int currentEmployeeId,
            int roleLevel,
            ExportType exportType,
            DateRangeReportFilterDto filter)
        {
            var fromDate = filter?.FromDate;
            var toDate = filter?.ToDate;
            var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);

            if (!fromDate.HasValue && !toDate.HasValue)
                return new List<TaskActivityReportDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) = await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);
            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel);

            var lastCommentIds = await _context.TaskComments
                .Where(c =>
                    (!fromDate.HasValue || c.CreatedDate >= fromDate.Value) &&
                    (!toDate.HasValue || c.CreatedDate <= toDate.Value))
                .GroupBy(c => new { c.TaskId, c.EmployeeId })
                .Select(g => g
                    .OrderByDescending(c => c.CreatedDate)
                    .ThenByDescending(c => c.Id)
                    .Select(c => c.Id)
                    .First())
                .ToListAsync();

            var query = _context.TaskComments
                .Include(c => c.Employee)
                .Include(c => c.Task)
                    .ThenInclude(t => t.AssignedBy)
                .Where(c => lastCommentIds.Contains(c.Id))
                .AsQueryable();

            query = query.Where(c =>
                c.Task.Assignments.Any(a =>
                    a.IsActive &&
                    scopedEmployeeIds.Contains(a.EmployeeId)));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(c =>
                        c.Task.Assignments.Any(a =>
                            a.IsActive &&
                            ( c.Task.CreatedByEmployeeId == currentEmployeeId))
                    );
                }
                else
                {
                    query = query.Where(c =>
                        c.Task.Assignments.Any(a =>
                            a.IsActive && a.EmployeeId == currentEmployeeId));
                }
            }

            var result = await query
                .Select(c => new TaskActivityReportDto
                {
                    TaskId = c.TaskId,
                    TaskTitleWithId = $"[{c.Task.Id}] {c.Task.Title}",
                    AssignedBy = c.Task.AssignedBy != null ? c.Task.AssignedBy.FullName : "غير معروف",
                    CommentDate = c.CreatedDate,
                    CommentedBy = c.Employee != null ? c.Employee.FullName : "غير معروف",
                    Comment =
                        exportType == ExportType.Pdf
                            ? (string.IsNullOrWhiteSpace(c.CommentText)
                                ? "رفع ملف"
                                : (c.CommentText.Length > 50 ? c.CommentText.Substring(0, 50) : c.CommentText))
                            : (string.IsNullOrWhiteSpace(c.CommentText) ? "رفع ملف" : c.CommentText)
                })
                .OrderByDescending(c => c.CommentDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<TaskMovementReportDto>> GetTaskMovementReportAsync(
    int currentEmployeeId,
    int roleLevel,
    TaskMovementReportFilterDto dto,
    ExportType exportType)
        {
            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            var lastCommentIds = await _context.TaskComments
                .Where(c => c.CreatedDate >= todayStart && c.CreatedDate < todayEnd)
                .GroupBy(c => new { c.TaskId, c.EmployeeId })
                .Select(g => g
                    .OrderByDescending(c => c.CreatedDate)
                    .ThenByDescending(c => c.Id)
                    .Select(c => c.Id)
                    .FirstOrDefault())
                .ToListAsync();

            var query = _context.TaskComments
                .Include(c => c.Employee)
                .Include(c => c.Task)
                    .ThenInclude(t => t.AssignedBy)
                .Where(c => lastCommentIds.Contains(c.Id))
                .AsQueryable();

            if (dto.MovementType == TaskMovementType.Outgoing && dto.EmployeeId.HasValue)
                query = query.Where(c => c.Task.AssignedByEmployeeId == dto.EmployeeId.Value);
            else if (dto.MovementType == TaskMovementType.Incoming && dto.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == dto.EmployeeId.Value);

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) = await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            query = query.Where(c =>
                c.Task.Assignments.Any(a =>
                    a.IsActive &&
                    scopedEmployeeIds.Contains(a.EmployeeId)));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    query = query.Where(c =>
                        c.Task.Assignments.Any(a =>
                            a.IsActive &&
                            (c.Task.CreatedByEmployeeId == currentEmployeeId))
                    );
                }
                else
                {
                    query = query.Where(c =>
                        c.Task.Assignments.Any(a =>
                            a.IsActive && scopedEmployeeIds.Contains(a.EmployeeId)));
                }
            }

            var result = await query
                .Select(c => new TaskMovementReportDto
                {
                    TaskId = c.TaskId,
                    ReportTitle = dto.ReportTitle,
                    TaskTitleWithId = $"[{c.Task.Id}] {c.Task.Title}",
                    MovementType = dto.MovementType,
                    AssignedBy = c.Task.AssignedBy != null ? c.Task.AssignedBy.FullName : "غير معروف",
                    CommentedBy = c.Employee != null ? c.Employee.FullName : "غير معروف",
                    CommentDate = c.CreatedDate,
                    CommentText =
                        exportType == ExportType.Pdf
                            ? (string.IsNullOrWhiteSpace(c.CommentText)
                                ? "رفع ملف"
                                : (c.CommentText.Length > 50 ? c.CommentText.Substring(0, 50) : c.CommentText))
                            : (string.IsNullOrWhiteSpace(c.CommentText) ? "رفع ملف" : c.CommentText),
                    AssignedTo = c.Task.Assignments
            .Where(a => a.IsActive)
            .Select(a => new EmployeeBriefDto
            {
                Id = a.EmployeeId,
                FullName = a.Employee.FullName
            })
            .Distinct()
            .ToList()


                })
                .OrderByDescending(c => c.CommentDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<List<TasksClosingSoonDto>> GetTasksClosingSoonAsync(
            int currentEmployeeId,
            int roleLevel,
            int? employeeId,
            DateTime fromDate,
            DateTime toDate)
        {
            var query = _context.TaskAssignments
                .Include(a => a.Task).ThenInclude(t => t.AssignedBy)
                .Include(a => a.Employee).ThenInclude(e => e.Branch).ThenInclude(b => b.Area)
                .Include(a => a.Employee).ThenInclude(e => e.Company)
                .Where(a =>
                    a.IsActive &&
                    (a.Task.Status == WorkTaskStatus.New || a.Task.Status == WorkTaskStatus.InProgress) &&
                    a.Task.DueDate != null &&
                    a.Task.DueDate >= fromDate &&
                    a.Task.DueDate <= toDate
                )
                .AsQueryable();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) = await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (employeeId.HasValue && employeeId.Value > 0 && employeeId.Value != currentEmployeeId)
                {
                    query = query.Where(a => false);
                }
                else if (canViewCreatedTasks)
                {
                    query = query.Where(a =>
                       // scopedEmployeeIds.Contains(a.EmployeeId) ||
                        a.Task.CreatedByEmployeeId == currentEmployeeId
                    );
                }
                else
                {
                    query = query.Where(a =>a.EmployeeId == currentEmployeeId);
                }
            }
            else
            {
                if (employeeId.HasValue && employeeId.Value > 0)
                    query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            var result = await query
                .OrderBy(a => a.Task.DueDate)
                .Select(a => new TasksClosingSoonDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,
                    Status = a.Task.Status,
                    AssignedBy = a.Task.AssignedBy != null ? a.Task.AssignedBy.FullName : "غير معروف",
                    DueDate = a.Task.DueDate,
                    EmployeeName = a.Employee != null ? a.Employee.FullName : "غير معروف",
                    CompanyName = a.Employee.Company != null ? a.Employee.Company.Name : "-",
                    BranchName = a.Employee.Branch != null ? a.Employee.Branch.Name : "-",
                    AreaName = a.Employee.Branch != null && a.Employee.Branch.Area != null ? a.Employee.Branch.Area.Name : "-"
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        public async Task<List<EmployeeTaskTrackingReportDto>> GetEmployeeTaskTrackingAsync(
     int currentEmployeeId,
     int roleLevel,
     int? employeeId,
     DateTime fromDate,
     DateTime? toDate)
        {
            var effectiveToDate = toDate ?? DateTime.Now;

            IQueryable<TaskAssignment> query = _context.TaskAssignments
                .Include(a => a.Task)
                    .ThenInclude(t => t.AssignedBy)
                .Include(a => a.Employee)
                .AsQueryable();

            query = query.Where(a => a.Task.CreatedDate >= fromDate);

            query = query.Where(a => a.Task.CreatedDate <= effectiveToDate);

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            query = query.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    if (employeeId.HasValue && employeeId.Value > 0 && employeeId.Value != currentEmployeeId)
                    {
                        query = query.Where(a =>
                            a.EmployeeId == employeeId.Value
                        );
                    }
                    else
                    {
                        query = query.Where(a =>
                            a.Task.CreatedByEmployeeId == currentEmployeeId
                        );
                    }
                }
                else
                {
                    query = query.Where(a =>
                        a.EmployeeId == currentEmployeeId
                    );
                }
            }
            else
            {
                if (employeeId.HasValue && employeeId.Value > 0)
                    query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            query = query.Where(a => a.IsActive);

            var result = await query
                .Select(a => new EmployeeTaskTrackingReportDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title,

                    Status =
                        a.Task.Status == WorkTaskStatus.Archived ? "مؤرشفة" :
                        a.Task.Status == WorkTaskStatus.Closed ? "مغلقة" :
                        a.Task.Status == WorkTaskStatus.AutoClose ? "مغلقة تلقائيًا" :
                        a.Task.Status == WorkTaskStatus.InProgress ? "قيد التنفيذ" :
                        a.Task.Status == WorkTaskStatus.New ? "جديدة" :
                        "غير محدد",

                    CreatedDate = a.Task.CreatedDate,
                    ClosedAt = a.Task.DueDate,

                    AssignedBy = a.Task.AssignedBy != null ? a.Task.AssignedBy.FullName : "غير معروف",

                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee != null ? a.Employee.FullName : "غير معروف"
                })
                .OrderBy(r => r.EmployeeName)
                .ThenBy(r => r.CreatedDate)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }


        public async Task<List<BranchTaskReportRowDto>> GetBranchTasksReportAsync(
     int currentEmployeeId,
     int roleLevel,
     BranchTasksReportFilterDto dto)
        {
            var effectiveTo = dto.ToDate ?? DateTime.Now;

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            var flat = await _context.TaskAssignments
                .Include(a => a.Task).ThenInclude(t => t.AssignedBy)
                .Include(a => a.Employee)
                .Where(a => a.IsActive)
                .Where(a => scopedEmployeeIds.Contains(a.EmployeeId))
                .Where(a => a.Employee.BranchId == dto.BranchId)
                .Where(a => a.Task.CreatedDate >= dto.FromDate && a.Task.CreatedDate <= effectiveTo)
                .Where(a => (canViewAllTasks || hasAccessScope) ? true : a.EmployeeId == currentEmployeeId)
                .Select(a => new
                {
                    a.TaskId,
                    a.Task.Title,
                    AssignedBy = a.Task.AssignedBy != null ? a.Task.AssignedBy.FullName : "غير معروف",
                    a.Task.CreatedDate,
                    OriginalDueDate = a.Task.DueDate,
                    Status = a.Task.Status,

                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee != null ? a.Employee.FullName : "غير معروف",

                    ExtensionRequestsCount = _context.TaskExtensionRequests
                        .Count(r => r.TaskId == a.TaskId && !r.IsDeleted),

                    LastApprovedNewDueDate = _context.TaskExtensionRequests
                        .Where(r => r.TaskId == a.TaskId
                                    && !r.IsDeleted
                                    && r.Status == ExtensionRequestStatus.Approved)
                        .OrderByDescending(r => r.CreatedDate)
                        .Select(r => (DateTime?)r.NewDueDate)
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync();

            var result = flat
                .GroupBy(x => new
                {
                    x.TaskId,
                    x.Title,
                    x.AssignedBy,
                    x.CreatedDate,
                    x.OriginalDueDate,
                    x.Status
                })
                .Select(g =>
                {
                    var employees = g
                        .Select(e => new EmployeeMiniDto { Id = e.EmployeeId, Name = e.EmployeeName })
                        .GroupBy(e => e.Id)
                        .Select(gg => gg.First())
                        .ToList();

                    var lastApproved = g.Select(x => x.LastApprovedNewDueDate).FirstOrDefault();
                    var extCount = g.Select(x => x.ExtensionRequestsCount).FirstOrDefault();

                    return new BranchTaskReportRowDto
                    {
                        TaskId = g.Key.TaskId,

                        Title = g.Key.Title,
                        AssignedBy = g.Key.AssignedBy,
                        CreatedDate = g.Key.CreatedDate,

                        OriginalDueDate = g.Key.OriginalDueDate,
                        EffectiveDueDate = lastApproved ?? g.Key.OriginalDueDate,
                        ExtensionRequestsCount = extCount,

                        StatusText =
                            g.Key.Status == WorkTaskStatus.Archived ? "مؤرشفة" :
                            g.Key.Status == WorkTaskStatus.Closed ? "مغلقة" :
                            g.Key.Status == WorkTaskStatus.AutoClose ? "مغلقة تلقائيًا" :
                            g.Key.Status == WorkTaskStatus.InProgress ? "قيد التنفيذ" :
                            g.Key.Status == WorkTaskStatus.New ? "جديدة" :
                            "غير محدد",

                        Employees = employees
                    };
                })
                .OrderBy(x => x.Title)
                .ThenBy(x => x.CreatedDate)
                .ThenBy(x => x.TaskId)
                .ToList();

            return result;
        }

        public async Task<List<EmployeeAssignedTaskOptionDto>> GetEmployeeAssignedTasksAsync(
            int currentEmployeeId,
            int roleLevel,
            int employeeId)
        {
            if (employeeId <= 0)
                return new List<EmployeeAssignedTaskOptionDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            // Must be within scope
            if (!await scopedEmployeeIds.ContainsAsync(employeeId))
                return new List<EmployeeAssignedTaskOptionDto>();

            IQueryable<TaskAssignment> query = _context.TaskAssignments
                .Include(a => a.Task)
                .Where(a =>
                    a.IsActive &&
                    a.EmployeeId == employeeId &&
                    !a.Task.IsDeleted);

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    // Allow viewing tasks you created assigned to that employee
                    if (employeeId != currentEmployeeId)
                        query = query.Where(a => a.Task.CreatedByEmployeeId == currentEmployeeId);
                }
                else
                {
                    // Only self
                    if (employeeId != currentEmployeeId)
                        return new List<EmployeeAssignedTaskOptionDto>();
                }
            }

            var tasks = await query
                .Select(a => new EmployeeAssignedTaskOptionDto
                {
                    TaskId = a.TaskId,
                    Title = a.Task.Title
                })
                .Distinct()
                .OrderBy(x => x.TaskId)
                .AsNoTracking()
                .ToListAsync();

            return tasks;
        }

        public async Task<List<EmployeeTaskCommentRowDto>> GetEmployeeTaskCommentsAsync(
            int currentEmployeeId,
            int roleLevel,
            int employeeId,
            int taskId,
            ExportType exportType)
        {
            if (employeeId <= 0 || taskId <= 0)
                return new List<EmployeeTaskCommentRowDto>();

            var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =
                await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel);

            // Must be within scope
            if (!await scopedEmployeeIds.ContainsAsync(employeeId))
                return new List<EmployeeTaskCommentRowDto>();

            var commentsQuery = _context.TaskComments
                .Include(c => c.Task)
                .Where(c =>
                    c.EmployeeId.HasValue &&
                    c.EmployeeId.Value == employeeId &&
                    c.TaskId == taskId &&
                    !c.Task.IsDeleted)
                .AsQueryable();

            if (!canViewAllTasks && !hasAccessScope)
            {
                if (canViewCreatedTasks)
                {
                    if (employeeId != currentEmployeeId)
                        commentsQuery = commentsQuery.Where(c => c.Task.CreatedByEmployeeId == currentEmployeeId);
                }
                else
                {
                    if (employeeId != currentEmployeeId)
                        return new List<EmployeeTaskCommentRowDto>();
                }
            }

            var result = await commentsQuery
                .OrderByDescending(c => c.CreatedDate)
                .ThenByDescending(c => c.Id)
                .Select(c => new EmployeeTaskCommentRowDto
                {
                    CommentId = c.Id,
                    CommentDate = c.CreatedDate,
                    CommentText =
                        exportType == ExportType.Pdf
                            ? (string.IsNullOrWhiteSpace(c.CommentText)
                                ? "رفع ملف"
                                : (c.CommentText.Length > 50 ? c.CommentText.Substring(0, 50) : c.CommentText))
                            : (string.IsNullOrWhiteSpace(c.CommentText) ? "رفع ملف" : c.CommentText)
                })
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

    }
}
