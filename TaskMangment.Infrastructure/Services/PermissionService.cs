using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<Permission> _permissionRepo;

        public PermissionService(
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IRepository<Permission> permissionRepo)
        {
            _employeeRoleRepo = employeeRoleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionRepo = permissionRepo;
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string permissionCode)
        {
            var permission = await _permissionRepo.GetAll(p => p.Code == permissionCode).FirstOrDefaultAsync();
            if (permission == null) return false;

            int permissionId = permission.Id;

            var userRoles = await _employeeRoleRepo.GetAll(er => er.EmployeeId == userId).Select(er => er.RoleId).ToListAsync();

            if (!userRoles.Any()) return false;

            bool hasPermission = await _rolePermissionRepo
                .GetAll(rp => userRoles.Contains(rp.RoleId) && rp.PermissionId == permissionId)
                .AnyAsync();

            return hasPermission;
        }
    }
}
