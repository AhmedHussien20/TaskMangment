using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public PermissionService(
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IRepository<Permission> permissionRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _employeeRoleRepo = employeeRoleRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _permissionRepo = permissionRepo;
            _mapper = mapper;
            _cache = cache;
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

        public async Task<ApiResponse<PagedResponse<PermissionGetDto>>> GetAllAsync(PermissionRequest request)
        {
          
            string cacheKey =
                $"permissions:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<PermissionGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<PermissionGetDto>>.Ok(cached);
            }

            var query = _permissionRepo.GetAll().AsNoTracking().AsQueryable();

          
            int totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<PermissionGetDto>>(list);

            var response = new PagedResponse<PermissionGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<PermissionGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<PermissionGetDto>> CreateAsync(PermissionAddDto dto)
        {
            if (await _permissionRepo.GetAll(p => p.Code == dto.Code).AnyAsync())
                return ApiResponse<PermissionGetDto>.Fail("Permission code already exists");

            var permission = _mapper.Map<Permission>(dto);
            await _permissionRepo.AddAsync(permission);
            await _permissionRepo.SaveChangesAsync();

          

            var resultDto = _mapper.Map<PermissionGetDto>(permission);
            await _cache.RemoveAsync("permissions:");

            return ApiResponse<PermissionGetDto>.Ok(resultDto, "Permission created successfully");
        }

        public async Task<ApiResponse<PermissionGetDto>> UpdateAsync(int id, PermissionAddDto dto)
        {
            var permission = await _permissionRepo.GetByIDAsync(id);
            if (permission == null)
                return ApiResponse<PermissionGetDto>.Fail("Permission not found", StatusCode.NotFound);

            if (permission.Code != dto.Code)
            {
                bool codeExists = await _permissionRepo.GetAll(p => p.Code == dto.Code).AnyAsync();
                if (codeExists)
                    return ApiResponse<PermissionGetDto>.Fail("Permission code already exists");
            }

            _mapper.Map(dto, permission);
            await _permissionRepo.SaveChangesAsync();
            await _cache.RemoveAsync("permissions:");



            var resultDto = _mapper.Map<PermissionGetDto>(permission);

            return ApiResponse<PermissionGetDto>.Ok(resultDto, "Permission updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var permission = await _permissionRepo.GetByIDAsync(id);
            if (permission == null)
                return ApiResponse<bool>.Fail("Permission not found", StatusCode.NotFound);

            _permissionRepo.SoftDelete(permission);
            await _permissionRepo.SaveChangesAsync();
            await _cache.RemoveAsync("permissions:");

            return ApiResponse<bool>.Ok(true, "Permission deleted successfully");
        }


    }
}
