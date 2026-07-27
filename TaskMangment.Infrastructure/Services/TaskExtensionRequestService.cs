using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using TaskMangment.Application.Common.Validation;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskExtensionRequestService : ITaskExtensionRequestsService
    {
        private readonly IRepository<TaskExtensionRequest> _requestRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IGetHigherManager _getHigherManager;


        public TaskExtensionRequestService(
            IRepository<TaskExtensionRequest> requestRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IRepository<Employee> employeeRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo,
            IDomainEventDispatcher eventDispatcher,
            IGetHigherManager getHigherManager)
        {
            _requestRepo = requestRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _employeeRepo = employeeRepo;
            _mapper = mapper;
            _cache = cache;
            _taskRepo = taskRepo;
            _eventDispatcher = eventDispatcher;
            _getHigherManager = getHigherManager;
        }

        public async Task<ApiResponse<PagedResponse<TaskExtensionRequestListDto>>> GetAllAsync(TaskExtensionRequestRequest request)
        {
            //string cacheKey = $"taskExtensionRequests:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<TaskExtensionRequestListDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<TaskExtensionRequestListDto>>.Ok(cached);
            //}

            var task = await _taskRepo.GetByIDAsync(request.TaskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);


            var query = _requestRepo.GetAll(c => c.TaskId == request.TaskId)
                                                      .Include(r => r.RequestedBy)
                                                     .Include(r => r.ReviewedBy)

                                                    .Include(r => r.TaskAssignment)
                                                    .ThenInclude(a => a.Employee)
                                                    .Include(r => r.TaskAssignment)
                                                    .ThenInclude(a => a.Task)

                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<TaskExtensionRequestListDto>>(list);

            var response = new PagedResponse<TaskExtensionRequestListDto>(dtos, totalCount, request.PageIndex, request.PageSize);

           // await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<TaskExtensionRequestListDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskExtensionRequestDetailsDto>> GetByIdAsync(int id)
        {
            var request = await _requestRepo.GetAll(r => r.Id == id)
                .Include(r => r.TaskAssignment)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReviewedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (request == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var savedRequest = await _requestRepo.GetAll(r => r.Id == request.Id)
                                                             .Include(r => r.RequestedBy)
                                                             .Include(r => r.TaskAssignment)
                                                             .ThenInclude(a => a.Employee)
                                                             .Include(r => r.TaskAssignment)
                                                             .ThenInclude(a => a.Task)
                                                            .AsNoTracking()
                                                            .FirstOrDefaultAsync();

            var resultDto = _mapper.Map<TaskExtensionRequestDetailsDto>(savedRequest);
            resultDto.TaskTitle = savedRequest.TaskAssignment?.Task?.Title;
            resultDto.ReviewedByName = savedRequest.ReviewedBy?.FullName;


            return ApiResponse<TaskExtensionRequestDetailsDto>.Ok(resultDto);
        }

        public async Task<ApiResponse<TaskExtensionRequestDetailsDto>> AddAsync(TaskExtensionRequestAddDto dto, int taskId, int employeeId)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var assignment = await _taskAssignmentRepo.GetAll(a => a.TaskId == taskId && a.EmployeeId == employeeId  && a.IsActive)
                                                      .FirstOrDefaultAsync();
            if (assignment == null)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);

            if (assignment.IsClosed)
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);

            var request = _mapper.Map<TaskExtensionRequest>(dto);
            request.TaskAssignmentId = assignment.Id;
            request.TaskId = taskId;
            request.RequestedByEmployeeId = employeeId;
            request.RequestedAt = DateTime.UtcNow;
            request.Status = ExtensionRequestStatus.Pending;

            await _requestRepo.AddAsync(request);
            await _requestRepo.SaveChangesAsync();
            await _cache.RemoveAsync("taskExtensionRequests:");


            var assignedEmployeeIds = await _taskAssignmentRepo
         .GetAll(a => a.TaskId == taskId && a.IsActive)
   .Select(a => a.EmployeeId)
   .ToListAsync();

            var employeeName = await _employeeRepo.GetAll(e => e.Id == employeeId).Select(e => e.FullName).FirstOrDefaultAsync();

            if (task.AssignedByEmployeeId.HasValue && !assignedEmployeeIds.Contains(task.AssignedByEmployeeId.Value))
            {
                assignedEmployeeIds.Add(task.AssignedByEmployeeId.Value);
            }
            assignedEmployeeIds.Remove(employeeId);
            assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();

            foreach (var recipientId in assignedEmployeeIds.ToList())
            {
                var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(recipientId);
                assignedEmployeeIds.AddRange(managerIds.Where(id => !assignedEmployeeIds.Contains(id)));
            }

            assignedEmployeeIds.Remove(employeeId);
            assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                .Select(e => e.Id)
                .ToListAsync();


            if (assignedEmployeeIds.Any())
            {
                await _eventDispatcher.PublishAsync(
                    new TaskExtensionRequestEvent(request.Id, taskId, employeeName, assignedEmployeeIds, task.Title)
                );
            }


            var savedRequest = await _requestRepo.GetAll(r => r.Id == request.Id)
                                                 .Include(r => r.RequestedBy)
                                                 .Include(r => r.TaskAssignment)
                                                 .ThenInclude(a => a.Employee) 
                                                 .Include(r => r.TaskAssignment)
                                                 .ThenInclude(a => a.Task) 
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();


            var resultDto = _mapper.Map<TaskExtensionRequestDetailsDto>(savedRequest);
            resultDto.TaskTitle = savedRequest.TaskAssignment.Task?.Title; 



            return ApiResponse<TaskExtensionRequestDetailsDto>.Ok(resultDto, "Extension request added successfully");
        }

        public async Task<ApiResponse<TaskExtensionRequestDetailsDto>> ReviewAsync(int id,TaskExtensionReviewDto dto,int reviewerId)
        {
            var request = await _requestRepo
                .GetAll(r => r.Id == id)
                .Include(r => r.TaskAssignment)
                .ThenInclude(a => a.Task)
                .FirstOrDefaultAsync();

            if (request == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var task = request.TaskAssignment?.Task;

            if (task == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            if (task.Status is WorkTaskStatus.Closed
                or WorkTaskStatus.AutoClose
                or WorkTaskStatus.Archived)
            {
                throw new AppException(
                    ErrorCodes.TaskAlreadyClosed,
                    StatusCodes.Status400BadRequest
                );
            }

            if (request.Status != ExtensionRequestStatus.Pending)
                throw new AppException(ErrorCodes.AlreadyReviewed, StatusCodes.Status400BadRequest);

            if (dto.Status == ExtensionRequestStatus.Approved)
            {
                if (!dto.NewDueDate.HasValue ||
                    dto.NewDueDate < task.DueDate)
                {
                    throw new AppException(ErrorCodes.InvalidDate, StatusCodes.Status400BadRequest);
                }

                TaskDueDateRules.EnsureValidDueDate(dto.NewDueDate);

                request.Status = ExtensionRequestStatus.Approved;
                request.NewDueDate = dto.NewDueDate.Value;

                var assignedEmployeeIds = await _taskAssignmentRepo
                    .GetAll(a => a.TaskId == task.Id && a.IsActive)
                    .Select(a => a.EmployeeId)
                    .ToListAsync();

                if (task.AssignedByEmployeeId.HasValue &&
                    !assignedEmployeeIds.Contains(task.AssignedByEmployeeId.Value))
                {
                    assignedEmployeeIds.Add(task.AssignedByEmployeeId.Value);
                }

                assignedEmployeeIds.Remove(reviewerId);
                assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                    .Select(e => e.Id)
                    .ToListAsync();

                foreach (var recipientId in assignedEmployeeIds.ToList())
                {
                    var managerIds = await _getHigherManager.GetDirectHigherManagerIdsAsync(recipientId);
                    assignedEmployeeIds.AddRange(managerIds.Where(id => !assignedEmployeeIds.Contains(id)));
                }

                assignedEmployeeIds.Remove(reviewerId);
                assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                    .Select(e => e.Id)
                    .ToListAsync();

                if (assignedEmployeeIds.Any())
                {
                    await _eventDispatcher.PublishAsync(
                        new TaskExtendApproveEvent(
                            request.Id,
                            task.Id,
                            task.Title,
                            task.DueDate,
                            dto.NewDueDate,
                            assignedEmployeeIds
                        )
                    );
                }
            }
            else
            {
                request.Status = ExtensionRequestStatus.Rejected;
            }

            request.ReviewedByEmployeeId = reviewerId;
            request.ReviewedAt = DateTime.UtcNow;

            await _requestRepo.SaveChangesAsync();
            await _cache.RemoveAsync("taskExtensionRequests:");

            var updatedRequest = await _requestRepo
                .GetAll(r => r.Id == id)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReviewedBy)
                .Include(r => r.TaskAssignment)
                .ThenInclude(a => a.Task)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var resultDto = _mapper.Map<TaskExtensionRequestDetailsDto>(updatedRequest);
            resultDto.TaskTitle = updatedRequest.TaskAssignment.Task?.Title;

            return ApiResponse<TaskExtensionRequestDetailsDto>
                .Ok(resultDto, "Request reviewed successfully");
        }

    }
}
