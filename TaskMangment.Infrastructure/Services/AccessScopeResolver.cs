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
        private readonly IUserAccessContextProvider _accessProvider;
        private readonly IEmployeePermissionService _permissions;
        private readonly IOrgManagerResolver _orgManagers;

        public AccessScopeResolver(
            IRepository<Employee> employeeRepo,
            IUserAccessContextProvider accessProvider,
            IEmployeePermissionService permissions,
            IOrgManagerResolver orgManagers)
        {
            _employeeRepo = employeeRepo;
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

            var access = await _accessProvider.GetAsync(employeeId);
            var perms = await _permissions.GetPermissionsAsync(employeeId);

            var companyWide =
                perms.Contains(PermissionCodes.ViewCompanyTasks) ||
                perms.Contains(PermissionCodes.ViewAllTasks) ||
                perms.Contains(PermissionCodes.ViewCompanyReports);

            AccessScopeKind kind;
            if (companyWide)
                kind = AccessScopeKind.CompanyWide;
            else if (access.BranchIds.Count > 0 || access.FunctionCodes.Count > 0)
                kind = AccessScopeKind.ManagerScoped;
            else if (perms.Contains(PermissionCodes.CreateTask) ||
                     perms.Contains(PermissionCodes.ViewScopedTasks) ||
                     perms.Contains(PermissionCodes.ViewEmployees))
                kind = AccessScopeKind.OwnBranch;
            else
                kind = AccessScopeKind.SelfOnly;

            return new ResolvedAccessScope
            {
                EmployeeId = employeeId,
                CompanyId = employee.CompanyId,
                OwnBranchId = employee.BranchId,
                Kind = kind,
                BranchIds = access.BranchIds,
                FunctionCodes = access.FunctionCodes
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
                    return query.ApplyAccessScope(scope.ToUserAccessContext());

                case AccessScopeKind.OwnBranch:
                    if (scope.OwnBranchId.HasValue)
                        return query.Where(e => e.BranchId == scope.OwnBranchId.Value);
                    return query.Where(e => e.Id == scope.EmployeeId);

                default:
                    return query.Where(e => e.Id == scope.EmployeeId);
            }
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
    }
}
