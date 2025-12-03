using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Job;
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
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public JobService(
            IRepository<Job> jobRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Department> departmentRepository,
            IMapper mapper,
            ICachingService cache)
        {
            _jobRepository = jobRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<JobGetDto>>> GetAllAsync(JobRequest request)
        {
            string safeTitle = request.Title ?? string.Empty;
            string safeEmployeeId = request.EmployeeId?.ToString() ?? "null";
            string safeDepartmentId = request.DepartmentId?.ToString() ?? "null";

            string cacheKey = $"jobs-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeTitle}-{safeEmployeeId}-{safeDepartmentId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<JobGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<JobGetDto>>.Ok(cached);
            }

            var query = _jobRepository.GetAll()
                .Include(j => j.Employees)
                .Include(j => j.Department)
                    .ThenInclude(d => d.Branch)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(j => j.Title.Contains(request.Title));

            if (request.EmployeeId.HasValue)
                query = query.Where(j => j.Employees.Any(e => e.Id == request.EmployeeId.Value));

            if (request.DepartmentId.HasValue)
                query = query.Where(j => j.DepartmentId == request.DepartmentId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<JobGetDto>>(list);

            var response = new PagedResponse<JobGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<JobGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<JobGetDto>> GetByIdAsync(int id)
        {
            var job = await _jobRepository.GetAll(j => j.Id == id)
                .Include(j => j.Employees)
                .Include(j => j.Department)
                    .ThenInclude(d => d.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (job == null)
                return ApiResponse<JobGetDto>.Fail("Job not found", StatusCode.NotFound);

            var dto = _mapper.Map<JobGetDto>(job);
            return ApiResponse<JobGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(JobAddEditDto dto)
        {
            if (!await _employeeRepository.IsExistAsync(dto.EmployeeId))
                return ApiResponse<bool>.Fail("Employee not found", StatusCode.NotFound);

            if (!await _departmentRepository.IsExistAsync(dto.DepartmentId))
                return ApiResponse<bool>.Fail("Department not found", StatusCode.NotFound);

            var job = _mapper.Map<Job>(dto);

            var employee = await _employeeRepository.GetByIDAsync(dto.EmployeeId);
            job.Employees.Add(employee);

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Job added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, JobAddEditDto dto)
        {
            var job = await _jobRepository.GetAll(j => j.Id == id)
                .Include(j => j.Employees)
                .FirstOrDefaultAsync();

            if (job == null)
                return ApiResponse<bool>.Fail("Job not found", StatusCode.NotFound);

            if (!await _employeeRepository.IsExistAsync(dto.EmployeeId))
                return ApiResponse<bool>.Fail("Employee not found", StatusCode.NotFound);

            if (!await _departmentRepository.IsExistAsync(dto.DepartmentId))
                return ApiResponse<bool>.Fail("Department not found", StatusCode.NotFound);

            _mapper.Map(dto, job);

            // Update Employee
            job.Employees.Clear();
            var employee = await _employeeRepository.GetByIDAsync(dto.EmployeeId);
            job.Employees.Add(employee);

            await _jobRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Job updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var job = await _jobRepository.GetByIDAsync(id);
            if (job == null)
                return ApiResponse<bool>.Fail("Job not found", StatusCode.NotFound);

            _jobRepository.SoftDelete(job);
            await _jobRepository.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Job deleted successfully");
        }
    }
}
