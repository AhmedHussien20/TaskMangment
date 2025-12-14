using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public RoleService(
            IRepository<Role> roleRepo,
            IRepository<Permission> permissionRepo,
            IRepository<RolePermission> rolePermRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<Employee> employeeRepo)
        {
            _roleRepo = roleRepo;
            _permissionRepo = permissionRepo;
            _rolePermRepo = rolePermRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _mapper = mapper;
            _cache = cache;
            _employeeRepo = employeeRepo;
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

        public async Task<ApiResponse<bool>> AssignPermissionsAsync(RolePermissionAssignDto dto)
        {
            if (!await _roleRepo.IsExistAsync(dto.RoleId))
                return ApiResponse<bool>.Fail("role not found", StatusCode.NotFound);

            if (dto.PermissionIds == null || !dto.PermissionIds.Any())
                return ApiResponse<bool>.Fail("no permissions to assign");

            foreach (var pid in dto.PermissionIds)
            {
                if (!await _permissionRepo.IsExistAsync(pid))
                    return ApiResponse<bool>.Fail("permission not found", StatusCode.NotFound);
            }

            foreach (var pid in dto.PermissionIds)
            {
                var rp = new RolePermission
                {
                    RoleId = dto.RoleId,
                    PermissionId = pid,
                    CreatedDate= DateTime.UtcNow
                };

                await _rolePermRepo.AddAsync(rp);
            }

            await _rolePermRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true,"Permissions assigned");
        }

        public async Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(AssignRoleToEmployeeDto dto)
        {

            if (!await _employeeRepo.IsExistAsync(dto.EmployeeId))
                return ApiResponse<bool>.Fail("employee not found", StatusCode.NotFound);

            if (!await _roleRepo.IsExistAsync(dto.RoleId))
                return ApiResponse<bool>.Fail("this role not found", StatusCode.NotFound);

            var alreadyAssigned = await _employeeRoleRepo
                   .GetAll(er => er.EmployeeId == dto.EmployeeId && er.RoleId == dto.RoleId)
                   .AnyAsync();

            if (alreadyAssigned)
                return ApiResponse<bool>.Fail("Role already assigned");

            var er = new EmployeeRole
            {
                EmployeeId = dto.EmployeeId,
                RoleId = dto.RoleId,
                CreatedDate = DateTime.UtcNow
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

