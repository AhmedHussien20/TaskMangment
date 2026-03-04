using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.Interfaces;

namespace TaskMangment.Infrastructure.Caching
{
    public class CacheInvalidator : ICacheInvalidator
    {
        private readonly ICachingService _cache;

        public CacheInvalidator(ICachingService cache)
        {
            _cache = cache;
        }

        private async Task BumpAsync(string versionKey)
        {
            var v = await _cache.GetAsync<int>(versionKey);
            if (v <= 0) v = 1;
            await _cache.SetAsync(versionKey, v + 1, TimeSpan.FromDays(30));
        }

        public Task InvalidateDashboardAsync(int companyId)
            => BumpAsync(CacheKeys.DashboardVersion(companyId));

        public Task InvalidateTasksAsync(int companyId)
            => BumpAsync(CacheKeys.TasksVersion(companyId));

        public Task InvalidateEmployeesAsync(int companyId)
            => BumpAsync(CacheKeys.EmployeesVersion(companyId));

        public Task InvalidateAreasAsync(int companyId)
            => BumpAsync(CacheKeys.AreasVersion(companyId));

        public Task InvalidateBranchAsync(int companyId)
            => BumpAsync(CacheKeys.BranchesVersion(companyId));

        public Task InvalidateEmployeeDashboardAsync(int employeeId)
            => BumpAsync(CacheKeys.EmployeeDashboardVersion(employeeId));

        public Task InvalidateDepartmentsAsync(int? branchId)
    => BumpAsync(CacheKeys.DepartmentsVersion(branchId));

        public Task InvalidateTaskCommentsAsync(int taskId)
            => BumpAsync(CacheKeys.TaskCommentsVersion(taskId));

        public Task InvalidateTaskCloseRequestsAsync(int taskId)
            => BumpAsync(CacheKeys.TaskCloseRequestsVersion(taskId));

        public Task InvalidateTaskDiscountsAsync(int taskId)
            => BumpAsync(CacheKeys.TaskDiscountsVersion(taskId));

        public Task InvalidateTaskExtensionRequestsAsync(int taskId)
            => BumpAsync(CacheKeys.TaskExtensionRequestsVersion(taskId));

        public Task InvalidateTaskWarningsAsync(int taskId)
            => BumpAsync(CacheKeys.TaskWarningsVersion(taskId));

        public Task InvalidateLeavesAsync(int companyId)
            => BumpAsync(CacheKeys.LeavesVersion(companyId));

        public Task InvalidateRolesAsync(int companyId)
            => BumpAsync(CacheKeys.RolesVersion(companyId));

        public Task InvalidateRoleAssignmentsAsync(int roleId)
            => BumpAsync(CacheKeys.RoleAssignmentsVersion(roleId));

    }


}
