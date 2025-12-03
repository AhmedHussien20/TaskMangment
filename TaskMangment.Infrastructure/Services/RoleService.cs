using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace TaskMangment.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;

        public RoleService(
            IRepository<Role> roleRepo,
            IRepository<Permission> permissionRepo,
            IRepository<RolePermission> rolePermRepo,
            IRepository<EmployeeRole> employeeRoleRepo)
        {
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _rolePermRepo = rolePermRepo;
            _employeeRoleRepo = employeeRoleRepo;
        }

        public async Task<ApiResponse<int>> CreateRoleAsync(RoleAddDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                CompanyId = dto.CompanyId,
                Description = dto.Description
            };

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveChangesAsync();

            return ApiResponse<int>.Ok(role.Id, "Role created");
        }

        public async Task<ApiResponse<bool>> AssignPermissionsAsync(int roleId, List<int> permissionIds)
        {
            foreach (var pid in permissionIds)
            {
                var rp = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pid
                };

                await _rolePermRepo.AddAsync(rp);
            }

            await _rolePermRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Permissions assigned");
        }

        public async Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(int employeeId, int roleId)
        {
            var er = new EmployeeRole
            {
                EmployeeId = employeeId,
                RoleId = roleId
            };

            await _employeeRoleRepo.AddAsync(er);
            await _employeeRoleRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Role assigned to employee");
        }

        public async Task<ApiResponse<List<RoleGetDto>>> GetRolesAsync(int companyId)
        {
            var roles = await _roleRepo.GetAll(r => r.CompanyId == companyId).ToListAsync();

            var result = roles.Select(r => new RoleGetDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

            return ApiResponse<List<RoleGetDto>>.Ok(result);
        }
    }
}

