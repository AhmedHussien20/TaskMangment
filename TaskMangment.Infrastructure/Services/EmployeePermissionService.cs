using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.Infrastructure.Services
{
    public class EmployeePermissionService : IEmployeePermissionService
    {
        private readonly AppDbContext _context;

        public EmployeePermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasAsync(int employeeId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
                return false;

            return await _context.EmployeeRoles
                .AsNoTracking()
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .SelectMany(er => er.Role.RolePermissions)
                .AnyAsync(rp =>
                    rp.IsAssigned &&
                    !rp.IsDeleted &&
                    rp.Permission != null &&
                    !rp.Permission.IsDeleted &&
                    rp.Permission.Code == permissionCode);
        }

        public async Task<bool> HasAnyAsync(int employeeId, params string[] permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
                return false;

            var codes = permissionCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            if (codes.Count == 0)
                return false;

            return await _context.EmployeeRoles
                .AsNoTracking()
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .SelectMany(er => er.Role.RolePermissions)
                .AnyAsync(rp =>
                    rp.IsAssigned &&
                    !rp.IsDeleted &&
                    rp.Permission != null &&
                    !rp.Permission.IsDeleted &&
                    codes.Contains(rp.Permission.Code));
        }

        public async Task<IReadOnlySet<string>> GetPermissionsAsync(int employeeId)
        {
            var list = await _context.EmployeeRoles
                .AsNoTracking()
                .Where(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted)
                .SelectMany(er => er.Role.RolePermissions)
                .Where(rp =>
                    rp.IsAssigned &&
                    !rp.IsDeleted &&
                    rp.Permission != null &&
                    !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            return list.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> HasActiveRoleAsync(int employeeId)
        {
            return _context.EmployeeRoles
                .AsNoTracking()
                .AnyAsync(er =>
                    er.EmployeeId == employeeId &&
                    er.IsAssigned &&
                    !er.IsDeleted &&
                    er.Role != null &&
                    !er.Role.IsDeleted);
        }
    }
}
