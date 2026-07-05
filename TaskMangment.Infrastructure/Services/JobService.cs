using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Job;
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
            string cacheKey = $"jobs:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

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
                .ApplySearch(request.searchKey);


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
                throw new AppException(ErrorCodes.JobNotFound,StatusCodes.Status400BadRequest);

            var dto = _mapper.Map<JobGetDto>(job);
            return ApiResponse<JobGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<JobGetDto>> AddAsync(JobAddEditDto dto)
        {
            if (!await _employeeRepository.IsExistAsync(dto.EmployeeId))
                throw new AppException(
                                                    ErrorCodes.EmployeeNotFound,
                                                    StatusCodes.Status400BadRequest);

            if (!await _departmentRepository.IsExistAsync(dto.DepartmentId))
                throw new AppException(ErrorCodes.DepartmentNotFound, StatusCodes.Status400BadRequest);

            var job = _mapper.Map<Job>(dto);

            var employee = await _employeeRepository.GetByIDAsync(dto.EmployeeId);
            job.Employees.Add(employee);

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();
            await _cache.RemoveAsync("jobs:");


            var fullJob = await _jobRepository.GetAll(j => j.Id == job.Id)
                                     .Include(j => j.Department)
                                     .Include(j => j.Employees)
                                     .Include(j => j.Department.Branch)
                                     .FirstOrDefaultAsync();

            var jobDto = _mapper.Map<JobGetDto>(fullJob);

            return ApiResponse<JobGetDto>.Ok(jobDto, "Job added successfully");
        }

        public async Task<ApiResponse<JobGetDto>> UpdateAsync(int id, JobAddEditDto dto)
        {
            var job = await _jobRepository.GetAll(j => j.Id == id)
                .Include(j => j.Employees)
                .FirstOrDefaultAsync();

            if (job == null)
                throw new AppException(ErrorCodes.JobNotFound, StatusCodes.Status400BadRequest);

            if (!await _employeeRepository.IsExistAsync(dto.EmployeeId))
                throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

            if (!await _departmentRepository.IsExistAsync(dto.DepartmentId))
                throw new AppException(ErrorCodes.DepartmentNotFound,StatusCodes.Status400BadRequest);

            _mapper.Map(dto, job);

            // Update Employee
            job.Employees.Clear();
            var employee = await _employeeRepository.GetByIDAsync(dto.EmployeeId);
            job.Employees.Add(employee);

            await _jobRepository.SaveChangesAsync();
            await _cache.RemoveAsync("jobs:");

            var fullJob = await _jobRepository.GetAll(j => j.Id == job.Id)
                                     .Include(j => j.Department)
                                     .Include(j => j.Employees)
                                     .Include(j => j.Department.Branch)
                                     .FirstOrDefaultAsync();

            var jobDto = _mapper.Map<JobGetDto>(fullJob);

            return ApiResponse<JobGetDto>.Ok(jobDto, "Job updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var job = await _jobRepository.GetByIDAsync(id);
            if (job == null)
                throw new AppException(ErrorCodes.JobNotFound, StatusCodes.Status400BadRequest);

            var hasEmployees = await _employeeRepository
                .GetAll(e => e.JobId == id)
                .AnyAsync();

            if (hasEmployees)
                throw new AppException(ErrorCodes.JobHasEmployees, StatusCodes.Status400BadRequest);

            _jobRepository.SoftDelete(job);
            await _jobRepository.SaveChangesAsync();
            await _cache.RemoveAsync("jobs:");

            return ApiResponse<bool>.Ok(true, "Job deleted successfully");
        }
    }
}
