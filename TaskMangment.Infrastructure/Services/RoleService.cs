using AutoMapper;
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
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public RoleService(
            IRepository<Role> roleRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<EmployeeRole> employeeRoleRepo,
            IRepository<RolePermission> rolePermissionRepo)
        {
            _roleRepo = roleRepo;
            _mapper = mapper;
            _cache = cache;
            _employeeRoleRepo = employeeRoleRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        public async Task<ApiResponse<PagedResponse<RoleGetDto>>> GetAllAsync(
    RoleRequest request,
    int companyId)
        {
            string cacheKey =
                $"roles:{companyId}:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<RoleGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<RoleGetDto>>.Ok(cached);
            }

            // Base query
            var query = _roleRepo
                .GetAll(r => r.CompanyId == companyId)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(
                request.SortColumn,
                request.SortDirection);

            var roles = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var roleIds = roles.Select(r => r.Id).ToList();
             
            var employeeCounts = await _employeeRoleRepo
                .GetAll(er => roleIds.Contains(er.RoleId))
                .GroupBy(er => er.RoleId)
                .Select(g => new
                {
                    RoleId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
             
            var permissionCounts = await _rolePermissionRepo
                .GetAll(rp => roleIds.Contains(rp.RoleId))
                .GroupBy(rp => rp.RoleId)
                .Select(g => new
                {
                    RoleId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
             
            var dtos = _mapper.Map<List<RoleGetDto>>(roles);

            foreach (var dto in dtos)
            {
                dto.EmployeeCount =
                    employeeCounts.FirstOrDefault(x => x.RoleId == dto.Id)?.Count ?? 0;

                dto.PermissionCount =
                    permissionCounts.FirstOrDefault(x => x.RoleId == dto.Id)?.Count ?? 0;
            }

            var response = new PagedResponse<RoleGetDto>(
                dtos,
                totalCount,
                request.PageIndex,
                request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<RoleGetDto>>.Ok(response);
        }


        public async Task<ApiResponse<RoleGetDto>> GetByIdAsync(int id, int companyId)
        {
            var role = await _roleRepo.GetAll(r =>
                    r.Id == id &&
                    r.CompanyId == companyId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var dto = _mapper.Map<RoleGetDto>(role);
            return ApiResponse<RoleGetDto>.Ok(dto);
        }
         
        public async Task<ApiResponse<int>> CreateAsync(
            RoleAddEditDto dto,
            int companyId)
        {
            var roleExists = await _roleRepo
                .GetAll(r =>
                    r.CompanyId == companyId &&
                    r.Name == dto.Name)
                .AnyAsync();

            if (roleExists)
                throw new AppException(ErrorCodes.AlreadyExists, StatusCodes.Status400BadRequest);


            var role = _mapper.Map<Role>(dto);
            role.CompanyId = companyId;

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveChangesAsync();

            await _cache.RemoveAsync("roles:");

            return ApiResponse<int>.Ok(role.Id, "Role created successfully");
        }
         
        public async Task<ApiResponse<RoleGetDto>> UpdateAsync(int id,RoleAddEditDto dto,int companyId)
        {
            var role = await _roleRepo.GetAll(r =>
                    r.Id == id &&
                    r.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            var roleExists = await _roleRepo
                .GetAll(r =>
                    r.CompanyId == companyId &&
                    r.Name == dto.Name &&
                    r.Id != id)
                .AnyAsync();

            if (roleExists)
                throw new AppException(ErrorCodes.AlreadyExists, StatusCodes.Status400BadRequest);

            dto.CompanyId = companyId;
            _mapper.Map(dto, role);

            await _roleRepo.SaveChangesAsync();
            await _cache.RemoveAsync("roles:");

            var dtoResult = _mapper.Map<RoleGetDto>(role);
            return ApiResponse<RoleGetDto>.Ok(dtoResult, "Role updated successfully");
        }

        
        public async Task<ApiResponse<bool>> DeleteAsync(int id, int companyId)
        {
            var role = await _roleRepo.GetAll(r =>
                    r.Id == id &&
                    r.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (role == null)
                throw new AppException(ErrorCodes.RoleNotFound, StatusCodes.Status400BadRequest);

            _roleRepo.SoftDelete(role);
            await _roleRepo.SaveChangesAsync();

            await _cache.RemoveAsync("roles:");

            return ApiResponse<bool>.Ok(true, "Role deleted successfully");
        }


    }

}

