using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Employee;
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
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<EmployeeRole> _employeeRoleRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public EmployeeService(
            IRepository<Employee> employeeRepo,
            IRepository<Role> roleRepo,
            IRepository<EmployeeRole> employeeRoleRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _employeeRepo = employeeRepo;
            _roleRepo = roleRepo;
            _employeeRoleRepo = employeeRoleRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<EmployeeGetDto>>> GetAllAsync(EmployeeRequest request)
        {
            string safeName = request.Name ?? string.Empty;
            string safeCompanyId = request.CompanyId?.ToString() ?? "null";
            string safeBranchId = request.BranchId?.ToString() ?? "null";

            string cacheKey =
                $"employees-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeName}-{safeCompanyId}-{safeBranchId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<EmployeeGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<EmployeeGetDto>>.Ok(cached);
            }

            var query = _employeeRepo.GetAll()
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(e => e.FullName.Contains(request.Name));

            if (request.CompanyId.HasValue)
                query = query.Where(e => e.CompanyId == request.CompanyId.Value);

            if (request.BranchId.HasValue)
                query = query.Where(e => e.BranchId == request.BranchId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            
            var dtos = _mapper.Map<ICollection<EmployeeGetDto>>(list);


            var response = new PagedResponse<EmployeeGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            // Save to cache for 10 minutes
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<EmployeeGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<EmployeeGetDto>> GetByIdAsync(int id)
        {
            var employee = await _employeeRepo.GetAll(e => e.Id == id)
                .Include(e => e.Branch)
                .Include(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (employee == null)
                return ApiResponse<EmployeeGetDto>.Fail("Employee not found");

            
            var dto = _mapper.Map<EmployeeGetDto>(employee);

            return ApiResponse<EmployeeGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(EmployeeAddEditDto dto)
        {
            var employee = _mapper.Map<Employee>(dto);

           
            // TODO: check for branch , company or remove

            var hasher = new PasswordHasher<Employee>();
            employee.PasswordHash = hasher.HashPassword(employee, dto.Password);

            await _employeeRepo.AddAsync(employee);
            await _employeeRepo.SaveChangesAsync();

            // Assign roles
            foreach (var roleId in dto.RoleIds)
            {
                if (!await _roleRepo.IsExistAsync(roleId))
                    return ApiResponse<bool>.Fail($"Role with ID {roleId} not found");

                await _employeeRoleRepo.AddAsync(new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = roleId
                });
            }

            await _employeeRoleRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, EmployeeAddEditDto dto)
        {
            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                return ApiResponse<bool>.Fail("Employee not found");

            _mapper.Map(dto, employee);

            // TODO: Hash password if provided
            // if(!string.IsNullOrWhiteSpace(dto.Password))
            //    employee.PasswordHash = HashPassword(dto.Password);

            // Update roles
            var oldRoles = await _employeeRoleRepo.GetAll(er => er.EmployeeId == id).ToListAsync();
            _employeeRoleRepo.DeleteRange(oldRoles);

            foreach (var roleId in dto.RoleIds)
            {
                if (!await _roleRepo.IsExistAsync(roleId))
                    return ApiResponse<bool>.Fail($"Role with ID {roleId} not found");

                await _employeeRoleRepo.AddAsync(new EmployeeRole
                {
                    EmployeeId = employee.Id,
                    RoleId = roleId
                });
            }

            await _employeeRepo.SaveChangesAsync();
            await _employeeRoleRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var employee = await _employeeRepo.GetByIDAsync(id);
            if (employee == null)
                return ApiResponse<bool>.Fail("Employee not found");

            _employeeRepo.SoftDelete(employee);
            await _employeeRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee deleted successfully");
        }
    }
}
