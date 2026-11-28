using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class AccessScopeResolver : IAccessScopeResolver
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Branch> _branchRepo;
        private readonly IRepository<Area> _areaRepo;
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IEmployeePermissionService _permissions;
        private readonly IOrgManagerResolver _orgManagers;

        public AccessScopeResolver(
            IRepository<Employee> employeeRepo,
            IRepository<Branch> branchRepo,
            IRepository<Area> areaRepo,
            IUserAccessContextProvider accessProvider,
            IEmployeePermissionService permissions,
            IOrgManagerResolver orgManagers)
        {
            _employeeRepo = employeeRepo;
            _branchRepo = branchRepo;
            _areaRepo = areaRepo;
            _accessProvider = accessProvider;
            _permissions = permissions;
            _orgManagers = orgManagers;
        }

        public async Task<ResolvedAccessScope> ResolveAsync(int employeeId)
        {
            var employee = await _employeeRepo.GetAll(e => e.Id == employeeId)
                .Select(e => new { e.Id, e.CompanyId, e.BranchId })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return new ResolvedAccessScope
                {
                    EmployeeId = employeeId,
                    Kind = AccessScopeKind.SelfOnly
                };
            }

            // No assigned role → self only. Ignore leftover Branch.ManagerID / functional-scope rows.
            if (!await _permissions.HasActiveRoleAsync(employeeId))
            {
                return new ResolvedAccessScope
                {
                    EmployeeId = employeeId,
                    CompanyId = employee.CompanyId,
                    OwnBranchId = employee.BranchId,
                    Kind = AccessScopeKind.SelfOnly,
                    BranchIds = new List<int>(),
                    EmployeeTypeIds = new List<int>(),
                    SeesAllTypesInBranchScope = false
                };
            }

            var access = await _accessProvider.GetAsync(employeeId);
            var perms = await _permissions.GetPermissionsAsync(employeeId);

            // Managed branches only (Branch.ManagerID / Area.ManagerEmployeeId).
            // Do not union home branch — managers whose home ≠ managed scope must not see home-branch staff.
            // OwnBranch roles (e.g. مشرف تدريب) still use OwnBranchId below when BranchIds is empty.
            var branchIds = access.BranchIds?.ToList() ?? new List<int>();

            var companyWide =
                perms.Contains(PermissionCodes.ViewCompanyTasks) ||
                perms.Contains(PermissionCodes.ViewCompanyReports);

            AccessScopeKind kind;
            if (companyWide)
                kind = AccessScopeKind.CompanyWide;
            else if (branchIds.Count > 0 || access.EmployeeTypeIds.Count > 0)
                kind = AccessScopeKind.ManagerScoped;
            else if (perms.Contains(PermissionCodes.CreateTask) ||
                     perms.Contains(PermissionCodes.ViewScopedTasks) ||
                     perms.Contains(PermissionCodes.ViewEmployees))
                kind = AccessScopeKind.OwnBranch;
            else
                kind = AccessScopeKind.SelfOnly;

            var viewExclude = kind is AccessScopeKind.ManagerScoped or AccessScopeKind.OwnBranch
                ? await BuildViewExcludeEmployeeIdsAsync(
                    employeeId,
                    employee.CompanyId,
                    employee.BranchId,
                    branchIds)
                : Array.Empty<int>();

            return new ResolvedAccessScope
            {
                EmployeeId = employeeId,
                CompanyId = employee.CompanyId,
                OwnBranchId = employee.BranchId,
                Kind = kind,
                BranchIds = branchIds,
                EmployeeTypeIds = access.EmployeeTypeIds,
                BranchRestrictedEmployeeTypeIds = access.BranchRestrictedEmployeeTypeIds,
                SeesAllTypesInBranchScope = access.SeesAllTypesInBranchScope,
                ViewExcludeEmployeeIds = viewExclude
            };
        }

        public IQueryable<Employee> FilterEmployees(
            IQueryable<Employee> query,
            ResolvedAccessScope scope,
            AccessIntent intent = AccessIntent.View)
        {
            // Do not force IsActive here — callers (employee list filter, task picker) decide.
            // Assign intent still excludes inactive employees.
            query = query.Where(e => e.CompanyId == scope.CompanyId && !e.IsDeleted);
            if (intent == AccessIntent.Assign)
                query = query.Where(e => e.IsActive);

            switch (scope.Kind)
            {
                case AccessScopeKind.CompanyWide:
                    return query;

                case AccessScopeKind.ManagerScoped:
                    query = query.ApplyAccessScope(scope.ToUserAccessContext());
                    break;

                case AccessScopeKind.OwnBranch:
                    if (scope.OwnBranchId.HasValue)
                        query = query.Where(e => e.BranchId == scope.OwnBranchId.Value);
                    else
                        query = query.Where(e => e.Id == scope.EmployeeId);
                    break;

                default:
                    return query.Where(e => e.Id == scope.EmployeeId);
            }

            // View only: hide org superiors / company-wide peers who share home branch.
            if (intent == AccessIntent.View &&
                scope.ViewExcludeEmployeeIds is { Count: > 0 } excludeIds)
            {
                query = query.Where(e => !excludeIds.Contains(e.Id));
            }

            return query;
        }

        public async Task<bool> CanViewEmployeeAsync(int actorId, int targetEmployeeId)
        {
            if (actorId == targetEmployeeId)
                return true;

            var scope = await ResolveAsync(actorId);
            return await FilterEmployees(_employeeRepo.GetAll(), scope)
                .AnyAsync(e => e.Id == targetEmployeeId);
        }

        public async Task<bool> CanAssignAsync(int actorId, int targetEmployeeId)
        {
            if (actorId == targetEmployeeId)
                return await _permissions.HasAsync(actorId, PermissionCodes.CreateTask);

            if (!await _permissions.HasAsync(actorId, PermissionCodes.CreateTask))
                return false;

            if (await _permissions.HasAsync(actorId, PermissionCodes.AssignOutsideScope))
                return true;

            var scope = await ResolveAsync(actorId);
            var inScope = await FilterEmployees(_employeeRepo.GetAll(), scope, AccessIntent.Assign)
                .AnyAsync(e => e.Id == targetEmployeeId);

            if (!inScope)
                return false;

            if (await _orgManagers.IsOrgManagerOverAsync(actorId, targetEmployeeId))
            {
                return await _permissions.HasAsync(actorId, PermissionCodes.AssignToManagers);
            }

            return true;
        }

        /// <summary>
        /// Org superiors (area managers over my units; home branch manager when I am neither
        /// that branch nor area manager) + company-wide users (VIEW_COMPANY_*).
        /// Area managers still see their subordinate branch managers.
        /// </summary>
        private async Task<IReadOnlyList<int>> BuildViewExcludeEmployeeIdsAsync(
            int actorId,
            int companyId,
            int? ownBranchId,
            IReadOnlyList<int> managedBranchIds)
        {
            var exclude = new HashSet<int>();

            var relevantBranchIds = new HashSet<int>(managedBranchIds);
            if (ownBranchId is int homeId && homeId > 0)
                relevantBranchIds.Add(homeId);

            if (relevantBranchIds.Count == 0)
                return exclude.ToList();

            var branchRows = await _branchRepo.GetAll(b =>
                    relevantBranchIds.Contains(b.Id) && !b.IsDeleted)
                .Select(b => new { b.Id, b.ManagerID, b.AreaId })
                .ToListAsync();

            var areaIds = branchRows
                .Where(b => b.AreaId.HasValue)
                .Select(b => b.AreaId!.Value)
                .Distinct()
                .ToList();

            if (areaIds.Count > 0)
            {
                var areaManagerIds = await _areaRepo.GetAll(a =>
                        areaIds.Contains(a.Id) &&
                        !a.IsDeleted &&
                        a.ManagerEmployeeId != null &&
                        a.ManagerEmployeeId != actorId)
                    .Select(a => a.ManagerEmployeeId!.Value)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in areaManagerIds)
                    exclude.Add(id);
            }

            // Home branch manager is above me only when I am not that branch/area manager.
            if (ownBranchId is int homeBranchId)
            {
                var home = branchRows.FirstOrDefault(b => b.Id == homeBranchId);
                if (home != null &&
                    home.ManagerID > 0 &&
                    home.ManagerID != actorId)
                {
                    var actorIsHomeAreaManager = home.AreaId.HasValue &&
                        await _areaRepo.GetAll(a =>
                                a.Id == home.AreaId.Value &&
                                !a.IsDeleted &&
                                a.ManagerEmployeeId == actorId)
                            .AnyAsync();

                    if (!actorIsHomeAreaManager)
                        exclude.Add(home.ManagerID);
                }
            }

            // Company-wide accounts (e.g. test Manager role) must not appear as "staff" of a scoped manager.
            var companyWideIds = await _employeeRepo.GetAll(e =>
                    e.CompanyId == companyId &&
                    !e.IsDeleted &&
                    e.Id != actorId &&
                    e.EmployeeRoles.Any(er =>
                        er.IsAssigned &&
                        !er.IsDeleted &&
                        er.Role != null &&
                        !er.Role.IsDeleted &&
                        er.Role.RolePermissions.Any(rp =>
                            rp.IsAssigned &&
                            !rp.IsDeleted &&
                            rp.Permission != null &&
                            !rp.Permission.IsDeleted &&
                            (rp.Permission.Code == PermissionCodes.ViewCompanyTasks ||
                             rp.Permission.Code == PermissionCodes.ViewCompanyReports))))
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var id in companyWideIds)
                exclude.Add(id);

            return exclude.ToList();
        }
    }
}
