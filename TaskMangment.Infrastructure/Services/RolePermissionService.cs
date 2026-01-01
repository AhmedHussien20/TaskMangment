using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.DataContext;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly ICachingService _cache;
        private readonly AppDbContext _context;
        public RolePermissionService(IRepository<Role> roleRepo,
            IRepository<RolePermission> rolePermRepo,
            IRepository<Permission> permissionRepo,
            ICachingService cache,
            AppDbContext context
)
        {
            _roleRepo = roleRepo;
            _rolePermRepo = rolePermRepo;
            _permissionRepo = permissionRepo;
            _cache = cache;
            _context = context;
        }

        public async Task<ApiResponse<PagedResponse<AssignedPermissionDto>>> GetAssignedPermissionsAsync(int roleId, RolePermissionRequest request)
        {
            if (!await _roleRepo.IsExistAsync(roleId))
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);
          

            //string cacheKey = $"assignedPermissions:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<AssignedPermissionDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<AssignedPermissionDto>>.Ok(cached);
            //}

            var query = _permissionRepo.GetAll().AsQueryable();

            if (!string.IsNullOrEmpty(request.searchKey))
                query = query.Where(p => p.Name.Contains(request.searchKey) || p.Code.Contains(request.searchKey));

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var permissions = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var assignedIds = await _rolePermRepo
                .GetAll(rp => rp.RoleId == roleId && !rp.IsDeleted)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var dtos = permissions.Select(p => new AssignedPermissionDto
            {
                PermissionId = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                IsAssigned = assignedIds.Contains(p.Id)
            }).ToList();

            var response = new PagedResponse<AssignedPermissionDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            //await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AssignedPermissionDto>>.Ok(response);
        }





        public async Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(int roleId, RolePermissionBulkAssignDto dto)
        {
            if (!await _roleRepo.IsExistAsync(roleId))
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var permissionIds = dto.Assignments.Select(a => a.PermissionId).Distinct().ToList();
            var existingPermissions = await _permissionRepo.GetAll(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var nonExistingPermissions = permissionIds.Except(existingPermissions).ToList();
            if (nonExistingPermissions.Any())
                throw new AppException(ErrorCodes.PermissionNotFound, StatusCodes.Status400BadRequest);

            foreach (var assignment in dto.Assignments)
            {
                var existingAssignment = await _rolePermRepo
                    .GetAll(rp => rp.PermissionId == assignment.PermissionId && rp.RoleId == roleId)
                    .FirstOrDefaultAsync();

                if (assignment.Assign)
                {
                    if (existingAssignment != null)
                    {
                        if (!existingAssignment.IsAssigned)
                        {
                            existingAssignment.IsAssigned = true;
                        }
                    }
                    else
                    {
                        await _rolePermRepo.AddAsync(new RolePermission
                        {
                            RoleId = roleId,
                            PermissionId = assignment.PermissionId,
                            CreatedDate = DateTime.UtcNow,
                            IsAssigned = true
                        });
                    }
                }
                else
                {
                    if (existingAssignment != null)
                    {
                        existingAssignment.IsAssigned = false;
                    }
                }
            }

            await _rolePermRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Permissions assigned/unassigned successfully");
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var permissions = await _context.EmployeeRoles
                .Where(er =>
                    er.EmployeeId == userId &&
                    !er.IsDeleted)
                .Select(er => er.RoleId)
                .Distinct()
                .Join(
                    _context.RolePermissions,
                    roleId => roleId,
                    rp => rp.RoleId,
                    (roleId, rp) => rp
                )
                .Where(rp =>
                    !rp.IsDeleted &&
                    rp.Permission != null &&
                    !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            return permissions;
        }


    }
}
