using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.Infrastructure.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly IEmployeePermissionService _permissions;

        public PermissionChecker(IEmployeePermissionService permissions)
        {
            _permissions = permissions;
        }

        public Task<bool> HasPermissionAsync(int employeeId, string permissionCode) =>
            _permissions.HasAsync(employeeId, permissionCode);
    }
}
