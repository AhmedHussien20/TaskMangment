using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskWarningService : ITaskWarningService
    {
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskWarningService(
            IRepository<Warning> warningRepo,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _warningRepo = warningRepo;
            _employeeRepo = employeeRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<WarningListDto>>> GetAllAsync(WarningRequest request)
        {
            string cacheKey = $"warnings:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<WarningListDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<WarningListDto>>.Ok(cached);
            }

            var query = _warningRepo.GetAll(c => c.TaskId == request.TaskId)
                .Include(w => w.IssuedBy)
                .Include(w => w.Issued)
                .Include(c => c.Task)
                .ApplySearch(request.searchKey);



            var totalCount = await query.CountAsync();
            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<WarningListDto>>(list);

            var response = new PagedResponse<WarningListDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<WarningListDto>>.Ok(response);
        }

        public async Task<ApiResponse<WarningGetDto>> GetByIdAsync(int id)
        {
            var warning = await _warningRepo.GetAll(w => w.Id == id)
                .Include(w => w.IssuedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (warning == null)
                return ApiResponse<WarningGetDto>.Fail("Warning not found");

            var dto = _mapper.Map<WarningGetDto>(warning);
            return ApiResponse<WarningGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<WarningGetDto>> AddAsync(WarningAddEditDto dto, int taskId, int employeeId)
        {

            var assignments = await _taskAssignmentRepo.GetAll(ta => ta.TaskId == taskId)
       .Include(ta => ta.Employee)
       .ToListAsync();

            if (assignments == null || !assignments.Any())
                return ApiResponse<WarningGetDto>.Fail("Task not found or has no assignments");

            var assignment = assignments.FirstOrDefault(a => a.TaskId == taskId && a.EmployeeId == dto.IssuedEmployeeId);

            if (assignment == null)
                return ApiResponse<WarningGetDto>.Fail("This employee is not assigned to this task");

            var warning = _mapper.Map<Warning>(dto);
            warning.TaskAssignmentId = assignment.Id;
            warning.TaskId = taskId;
            warning.IssuedByEmployeeId = employeeId;
            warning.CreatedBy = employeeId;
            warning.CreatedDate = DateTime.UtcNow;
            warning.IssuedEmployeeId= dto.IssuedEmployeeId;



            await _warningRepo.AddAsync(warning);
            await _warningRepo.SaveChangesAsync();
            await _cache.RemoveAsync("warnings:");


            var savedRequest = await _warningRepo.GetAll(r => r.Id == warning.Id)
                                               .Include(r => r.IssuedBy)
                                               .Include(r => r.Issued)
                                               .Include(r => r.TaskAssignment)
                                               .ThenInclude(a => a.Employee)
                                               .Include(r => r.TaskAssignment)
                                               .ThenInclude(a => a.Task)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync();

            var warningDto = _mapper.Map<WarningGetDto>(savedRequest);
            warningDto.TaskTitle = savedRequest.TaskAssignment.Task?.Title;
            warningDto.IssuedEmployeeName = savedRequest.Issued?.FullName;
            warningDto.IssuedByName = savedRequest.IssuedBy?.FullName;

            return ApiResponse<WarningGetDto>.Ok(warningDto, "Warning added successfully");
        }
        public async Task<ApiResponse<WarningGetDto>> UpdateAsync(int id, WarningAddEditDto dto)
        {
            var warning = await _warningRepo.GetAll(w => w.Id == id)
                .Include(w => w.TaskAssignment)
                .FirstOrDefaultAsync();

            if (warning == null)
                return ApiResponse<WarningGetDto>.Fail("Warning not found");

            

            var taskAssignments = await _taskAssignmentRepo.GetAll(ta => ta.TaskId == warning.TaskAssignment.TaskId)
                .ToListAsync();

            var assignment = taskAssignments.FirstOrDefault(ta => ta.EmployeeId == dto.IssuedEmployeeId);

            if (assignment == null)
                return ApiResponse<WarningGetDto>.Fail("This employee is not assigned to the task");

             warning = _mapper.Map<Warning>(dto);
            warning.TaskAssignmentId = assignment.Id;
            warning.ModifiedDate = DateTime.UtcNow;
            warning.IssuedEmployeeId = dto.IssuedEmployeeId;

            await _warningRepo.SaveChangesAsync();
            await _cache.RemoveAsync("warnings:");


            var savedRequest = await _warningRepo.GetAll(r => r.Id == warning.Id)
                                                           .Include(r => r.IssuedBy)
                                                           .Include(r => r.Issued)
                                                           .Include(r => r.TaskAssignment)
                                                           .ThenInclude(a => a.Employee)
                                                           .Include(r => r.TaskAssignment)
                                                           .ThenInclude(a => a.Task)
                                                          .AsNoTracking()
                                                          .FirstOrDefaultAsync();

            var warningDto = _mapper.Map<WarningGetDto>(savedRequest);
            warningDto.TaskTitle = savedRequest.TaskAssignment.Task?.Title;
            warningDto.IssuedEmployeeName = savedRequest.Issued?.FullName;
            warningDto.IssuedByName = savedRequest.IssuedBy?.FullName;

            return ApiResponse<WarningGetDto>.Ok(warningDto, "Warning updated successfully");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var warning = await _warningRepo.GetByIDAsync(id);
            if (warning == null)
                return ApiResponse<bool>.Fail("Warning not found");

            _warningRepo.SoftDelete(warning);
            await _warningRepo.SaveChangesAsync();
            await _cache.RemoveAsync("warnings:");

            return ApiResponse<bool>.Ok(true, "Warning deleted successfully");

        }
    }
}
