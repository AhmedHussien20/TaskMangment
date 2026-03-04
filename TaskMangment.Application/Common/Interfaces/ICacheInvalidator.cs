using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Application.Common.Interfaces
{
    public interface ICacheInvalidator
    {
        Task InvalidateDashboardAsync(int companyId);
        Task InvalidateTasksAsync(int companyId);
        Task InvalidateEmployeesAsync(int companyId);
        Task InvalidateAreasAsync(int companyId);
        Task InvalidateBranchAsync(int companyId);
        Task InvalidateEmployeeDashboardAsync(int employeeId);
        Task InvalidateDepartmentsAsync(int? branchId);
        Task InvalidateTaskCommentsAsync(int taskId);
        Task InvalidateTaskCloseRequestsAsync(int taskId);
        Task InvalidateTaskDiscountsAsync(int taskId);
        Task InvalidateTaskExtensionRequestsAsync(int taskId);
        Task InvalidateTaskWarningsAsync(int taskId);
        Task InvalidateLeavesAsync(int companyId);
        public Task InvalidateRolesAsync(int companyId);
        public Task InvalidateRoleAssignmentsAsync(int roleId);
    }

}
