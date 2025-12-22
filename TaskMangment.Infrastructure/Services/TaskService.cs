using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;
using TaskMangment.Infrastructure.SignalR;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskService: ITaskService
    {
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<EmailQueue> _emailQueueRepo;
        
        private readonly IRepository<TaskAssignment> _assignmentRepo;
        private readonly IRepository<AuditLog> _audit;
        private readonly IDomainEventDispatcher _eventDispatcher;

        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskService(
             IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<AuditLog> audit,
            IMapper mapper, 
            ICachingService cache ,
            IDomainEventDispatcher eventDispatcher,
            IRepository<EmailQueue> emailQueueRepo)
        {
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _assignmentRepo = assignmentRepo;
            _audit = audit;
            _mapper = mapper;
            _cache = cache;
            _eventDispatcher = eventDispatcher;
            _emailQueueRepo = emailQueueRepo;
        }

        public async Task<ApiResponse<PagedResponse<TaskGetDto>>> GetAllAsync(TaskRequest request, int CompanyId)
        {
            string cacheKey =
                $"tasks:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{CompanyId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<TaskGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<TaskGetDto>>.Ok(cached);
            }

            var query = _taskRepo.GetAll()
                .Include(t => t.CreatedBy)
                .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                .Include(t => t.AssignedBy)
                .ApplySearch(request.searchKey);



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
                if (task == null) continue;

                dto.AssignEmployee = task.Assignments
                    .Select(a => new TaskEmployeeAssignmentDto
                    {
                        Id = a.Employee.Id,
                        Name = a.Employee.FullName
                    })
                    .ToList();

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
                .Include(t => t.Assignments)
                .ThenInclude(a => a.Employee)
                .Include(t => t.AssignedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

            var fullTask = await _taskRepo.GetAll(t => t.Id == task.Id)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedBy)
                .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var taskDto = _mapper.Map<TaskGetDto>(fullTask); ;

            return ApiResponse<TaskGetDto>.Ok(taskDto);
        }

        public async Task<ApiResponse<TaskGetDto>> AddAsync(TaskAddEditDto dto, int createdUser, int companyId)
        {
            var task = _mapper.Map<WorkTask>(dto);
            task.CreatedByEmployeeId = createdUser;
            task.CompanyId = companyId;
            task.AssignedByEmployeeId = createdUser;
            task.AssignedBy= await _employeeRepo.GetByIDAsync(createdUser);

            await _taskRepo.AddAsync(task);
            await _taskRepo.SaveChangesAsync();

            foreach (var empId in dto.AssignedEmployeeIds)
            {
                if (!await _employeeRepo.IsExistAsync(empId))
                    throw new AppException(ErrorCodes.EmployeeNotFound, StatusCodes.Status400BadRequest);

                var assignment = new TaskAssignment
                {
                    TaskId = task.Id,
                    EmployeeId = empId,
                    IsActive = true
                };
                await _assignmentRepo.AddAsync(assignment);
            }
            await _assignmentRepo.SaveChangesAsync();
            await _cache.RemoveAsync("tasks:");
            foreach (var empId in dto.AssignedEmployeeIds)
            {
                var employee = await _employeeRepo.GetByIDAsync(empId);
                if (employee == null || string.IsNullOrEmpty(employee.Email))
                    continue;

                await _emailQueueRepo.AddAsync(new EmailQueue
                {
                    ToEmail = employee.Email,           
                    TemplateKey = "TaskAssigned",
                    ReferenceType = ReferenceType.Task,
                    ReferenceId = task.Id,
                    ScheduledAt = DateTime.UtcNow,
                    Status = EmailStatus.Pending,
                    UserId = empId
                });
            }
            await _emailQueueRepo.SaveChangesAsync();
            //Notfication
            await _eventDispatcher.PublishAsync(new TaskAssignedEvent(task.Id,task.Title,dto.AssignedEmployeeIds));

            var fullTask = await _taskRepo.GetAll(t => t.Id == task.Id)
                         .Include(t => t.CreatedBy)
                         .Include(t => t.AssignedBy)
                         .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                         .AsNoTracking()
                         .FirstOrDefaultAsync();

            var taskDto = _mapper.Map<TaskGetDto>(fullTask); ;

            return ApiResponse<TaskGetDto>.Ok(taskDto, "Task added successfully");
        }
        public async Task<ApiResponse<TaskGetDto>> UpdateAsync(int id, TaskAddEditDto dto, int modifierUser)
        {
            var task = await _taskRepo.GetByIDAsync(id);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

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
            await _cache.RemoveAsync("tasks:");

            var fullTask = await _taskRepo.GetAll(t => t.Id == task.Id)
      .Include(t => t.Company)
      .Include(t => t.CreatedBy)
      .Include(t => t.AssignedBy)
      .Include(t => t.Assignments)
          .ThenInclude(a => a.Employee)
      .Include(t => t.Comments)
      //.Include(t => t.Attachments)
      .Include(t => t.CloseRequests)
      .Include(t => t.ExtensionRequests)
      .FirstOrDefaultAsync();
            var taskDto = _mapper.Map<TaskGetDto>(fullTask);

            return ApiResponse<TaskGetDto>.Ok(taskDto, "Task updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var task = await _taskRepo.GetByIDAsync(id);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

            _taskRepo.SoftDelete(task);
            await _taskRepo.SaveChangesAsync();
            await _cache.RemoveAsync("tasks:");

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


        public async Task<ApiResponse<List<TaskAssignmentDto>>> GetAssignedEmployeesAsync(int taskId)
        {

            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

            var assignments = await _assignmentRepo.GetAll(a => a.TaskId == taskId)
            .Include(a => a.Employee)
           .ThenInclude(e => e.EmployeeRoles)   
               .ThenInclude(er => er.Role)   
                .ToListAsync();

            if (!assignments.Any())
                return ApiResponse<List<TaskAssignmentDto>>.Ok(new List<TaskAssignmentDto>());

            var dtos = assignments.Select(a => new TaskAssignmentDto
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FullName,
                Role = string.Join(", ", a.Employee.EmployeeRoles.Select(er => er.Role.Name)),
                Status = a.IsActive ? "Active" : "Inactive"
            }).ToList();

            return ApiResponse<List<TaskAssignmentDto>>.Ok(dtos);
        }

    }
}
