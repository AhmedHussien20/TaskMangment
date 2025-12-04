using AutoMapper;
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
    public class TaskService: ITaskService
    {
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<AuditLog> _audit;

        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskService(
             IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<AuditLog> audit,
            IMapper mapper,
            ICachingService cache)
        {
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _assignmentRepo = assignmentRepo;
            _audit = audit;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<TaskGetDto>>> GetAllAsync(TaskRequest request)
        {
            string safeTitle = request.Title ?? string.Empty;
            string safeCompanyId = request.CompanyId?.ToString() ?? "null";

            string cacheKey =
                $"tasks-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeTitle}-{safeCompanyId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<TaskGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<TaskGetDto>>.Ok(cached);
            }

            var query = _taskRepo.GetAll()
                .Include(t => t.CreatedBy)
                .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
                query = query.Where(t => t.Title.Contains(request.Title));

            if (request.CompanyId.HasValue)
                query = query.Where(t => t.CompanyId == request.CompanyId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<TaskGetDto>>(list);

            foreach (var dto in dtos)
            {
                var task = list.FirstOrDefault(t => t.Id == dto.Id);
                dto.EmployeeNames = task.Assignments.Select(a => a.Employee.FullName).ToList();
                dto.AssignedByName = task.AssignedBy?.FullName;
            }

            var response = new PagedResponse<TaskGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<TaskGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskGetDto>> GetByIdAsync(int id)
        {
            var task = await _taskRepo.GetAll(t => t.Id == id)
                .Include(t => t.CreatedBy)
                .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (task == null)
                return ApiResponse<TaskGetDto>.Fail("Task not found", StatusCode.NotFound);

            var dto = _mapper.Map<TaskGetDto>(task);
            dto.EmployeeNames = task.Assignments.Select(a => a.Employee.FullName).ToList();
            dto.AssignedByName = task.AssignedBy?.FullName;

            return ApiResponse<TaskGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(TaskAddEditDto dto)
        {
            var task = _mapper.Map<WorkTask>(dto);

            await _taskRepo.AddAsync(task);

            foreach (var empId in dto.AssignedEmployeeIds)
            {
                if (!await _employeeRepo.IsExistAsync(empId))
                    return ApiResponse<bool>.Fail($"Employee with ID {empId} not found");

                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    EmployeeId = empId,
                    IsActive = true
                };
                await _assignmentRepo.AddAsync(assignment);
            }

            await _assignmentRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Task added successfully");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, TaskAddEditDto dto)
        {
            var task = await _taskRepo.GetByIDAsync(id);
            if (task == null)
                return ApiResponse<bool>.Fail("Task not found", StatusCode.NotFound);

            _mapper.Map(dto, task);

            var existingAssignments = await _assignmentRepo
                .GetAll(a => a.TaskId == id)
                .ToListAsync();

            var newEmployeeIds = dto.AssignedEmployeeIds ?? new List<int>();

            foreach (var oldAssignment in existingAssignments)
            {
                if (!newEmployeeIds.Contains(oldAssignment.EmployeeId))
                {
                    oldAssignment.IsActive = false;
                }
            }

            foreach (var empId in newEmployeeIds)
            {
                var assignment = existingAssignments
                    .FirstOrDefault(a => a.EmployeeId == empId);

                if (assignment != null)
                {
                    assignment.IsActive = true;
                }
                else
                {
                    await _assignmentRepo.AddAsync(new TaskAssignment
                    {
                        TaskId = id,
                        EmployeeId = empId,
                        IsActive = true
                    });
                }
            }


            await _assignmentRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Task updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var task = await _taskRepo.GetByIDAsync(id);
            if (task == null)
                return ApiResponse<bool>.Fail("Task not found", StatusCode.NotFound);
            _taskRepo.SoftDelete(task);
            await _taskRepo.SaveChangesAsync();

            //need to know if we need to delete assignments also or cascade delete will handle it
            //var assignments = await _assignmentRepo.GetAll(a => a.TaskId == task.Id).ToListAsync();
            //foreach (var a in assignments)
            //{
            //    a.IsDeleted = true;
            //    a.DeletedDate = DateTime.UtcNow;
            //}
            //await _assignmentRepo.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Task deleted successfully");
        }
    }
}
