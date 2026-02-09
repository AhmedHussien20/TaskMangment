using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class PermissionChecker : IPermissionChecker
    {
        private readonly AppDbContext _context;

        public PermissionChecker(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(int employeeId, string permissionCode)
        {
            return await _context.EmployeeRoles
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted)
                .SelectMany(er => er.Role.RolePermissions)
                .AnyAsync(rp =>
                    rp.IsAssigned &&
                    !rp.IsDeleted &&
                    rp.Permission.Code == permissionCode);
        }
    }

}
