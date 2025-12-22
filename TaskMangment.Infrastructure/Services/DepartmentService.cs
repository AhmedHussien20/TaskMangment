using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
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
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Branch> _branchRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public DepartmentService(
            IRepository<Department> departmentRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Branch> branchRepository,
            IMapper mapper,
            ICachingService cache)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
            _branchRepository = branchRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<DepartmentGetDto>>> GetAllAsync(DepartmentRequest request)
        {
            string cacheKey = $"departments:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<DepartmentGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<DepartmentGetDto>>.Ok(cached);
            }

            var query = _departmentRepository.GetAll()
                .Include(d => d.Manager)
                .Include(d => d.Branch)
                    .ThenInclude(b => b.Area)
                .Include(d => d.Jobs)
                .ApplySearch(request.searchKey);
            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<DepartmentGetDto>>(list);

            foreach (var dto in dtos)
            {
                var department = list.First(d => d.Id == dto.Id);
                dto.EmployeeCount = department.Jobs.Count;
            }

            var response = new PagedResponse<DepartmentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<DepartmentGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<DepartmentGetDto>> GetByIdAsync(int id)
        {
            var department = await _departmentRepository.GetAll(d => d.Id == id)
                .Include(d => d.Manager)
                .Include(d => d.Branch)
                    .ThenInclude(b => b.Area)
                .Include(d => d.Jobs)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (department == null)
                throw new AppException(
                    ErrorCodes.DepartmentNotFound,
                    StatusCodes.Status404NotFound);
            var dto = _mapper.Map<DepartmentGetDto>(department);
            dto.EmployeeCount = department.Jobs.Count;

            return ApiResponse<DepartmentGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<DepartmentGetDto>> AddAsync(DepartmentAddEditDto dto)
        {
            if (!await _branchRepository.IsExistAsync(dto.BranchId))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status404NotFound);

            if (dto.ManagerEmployeeId.HasValue && !await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status404NotFound);

            var managerAlreadyUsed = await _departmentRepository
                .GetAll(d => d.ManagerEmployeeId == dto.ManagerEmployeeId)
                .AnyAsync();

            if (managerAlreadyUsed)
                throw new AppException(
                                    ErrorCodes.AlreadyAssigned,
                                    StatusCodes.Status400BadRequest);

            var department = _mapper.Map<Department>(dto);

            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveChangesAsync();
            await _cache.RemoveAsync("departments:");

            var fullDepartment = await _departmentRepository
      .GetAll(d => d.Id == department.Id)
      .Include(d => d.Branch)
          .ThenInclude(b => b.Area)
      .Include(d => d.Manager)
      .Include(d => d.Jobs)
      .FirstOrDefaultAsync();
            var departmentdto = _mapper.Map<DepartmentGetDto>(fullDepartment);

            return ApiResponse<DepartmentGetDto>.Ok(departmentdto, "Department added successfully");
        }

        public async Task<ApiResponse<DepartmentGetDto>> UpdateAsync(int id, DepartmentAddEditDto dto)
        {
            var department = await _departmentRepository.GetByIDAsync(id);
            if (department == null)
                throw new AppException(
                                    ErrorCodes.DepartmentNotFound,
                                    StatusCodes.Status400BadRequest);
            if (!await _branchRepository.IsExistAsync(dto.BranchId))
                throw new AppException(ErrorCodes.BranchNotFound, StatusCodes.Status400BadRequest);

            if (dto.ManagerEmployeeId.HasValue && !await _employeeRepository.IsExistAsync(dto.ManagerEmployeeId.Value))
                throw new AppException(ErrorCodes.ManagerNotFound, StatusCodes.Status400BadRequest);


            var managerAlreadyUsed = await _departmentRepository
                .GetAll(d => d.ManagerEmployeeId == dto.ManagerEmployeeId)
                .AnyAsync();

            if (managerAlreadyUsed)
                throw new AppException(ErrorCodes.AlreadyAssigned, StatusCodes.Status400BadRequest);

            _mapper.Map(dto, department);
            await _departmentRepository.SaveChangesAsync();
            await _cache.RemoveAsync("departments:");

            var fullDepartment = await _departmentRepository
                 .GetAll(d => d.Id == department.Id)
                 .Include(d => d.Branch)
                     .ThenInclude(b => b.Area)
                 .Include(d => d.Manager)
                 .Include(d => d.Jobs)
                 .FirstOrDefaultAsync();
            var departmentdto = _mapper.Map<DepartmentGetDto>(fullDepartment);
            return ApiResponse<DepartmentGetDto>.Ok(departmentdto, "Department updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var department = await _departmentRepository.GetByIDAsync(id);
            if (department == null)
                throw new AppException(
                                    ErrorCodes.DepartmentNotFound,
                                    StatusCodes.Status400BadRequest);
            _departmentRepository.SoftDelete(department);
            department.ManagerEmployeeId = null;

            await _departmentRepository.SaveChangesAsync();
            await _cache.RemoveAsync("departments:");


            return ApiResponse<bool>.Ok(true, "Department deleted successfully");
        }
    }
}
