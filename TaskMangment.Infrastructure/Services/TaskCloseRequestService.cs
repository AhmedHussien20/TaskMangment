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
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Caching;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{

    public class TaskCloseRequestService : ITaskCloseRequestService
    {
        private readonly IRepository<TaskCloseRequest> _requestRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly IRepository<TaskPercentage> _AchievementRepo;
        private readonly ICacheInvalidator _cacheInvalidator;




        public TaskCloseRequestService(
            IRepository<TaskCloseRequest> requestRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IDomainEventDispatcher eventDispatcher, IRepository<TaskPercentage> AchievementRepo, ICacheInvalidator cacheInvalidator)

        {
            _requestRepo = requestRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _mapper = mapper;
            _cache = cache;
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _eventDispatcher = eventDispatcher;
            _AchievementRepo = AchievementRepo;
            _cacheInvalidator = cacheInvalidator;
        }

        private async Task<int> GetVersionAsync(string versionKey)
        {
            var v = await _cache.GetAsync<int>(versionKey);
            if (v <= 0)
            {
                await _cache.SetAsync(versionKey, 1, TimeSpan.FromDays(30));
                return 1;
            }
            return v;
        }
        public async Task<ApiResponse<PagedResponse<TaskCloseRequestListDto>>> GetAllAsync(TaskCloseRequestRequest request)
        {
            var task = await _taskRepo.GetByIDAsync(request.TaskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var version = await GetVersionAsync(CacheKeys.TaskCloseRequestsVersion(request.TaskId));
            var cacheKey = CacheKeys.TaskCloseRequestsList(request.TaskId, request, version);

            var response = await _cache.GetOrSetAsync<PagedResponse<TaskCloseRequestListDto>>(
                cacheKey,
                async () =>
                {
                    var query = _requestRepo.GetAll(c => c.TaskId == request.TaskId)
                        .Include(r => r.TaskAssignment)
                        .Include(r => r.RequestedBy)
                        .Include(r => r.ReviewedBy)
                        .ApplySearch(request.searchKey);

                    var totalCount = await query.CountAsync();

                    query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

                    var list = await query
                        .Skip((request.PageIndex - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToListAsync();

                    var dtos = _mapper.Map<ICollection<TaskCloseRequestListDto>>(list);

                    return new PagedResponse<TaskCloseRequestListDto>(dtos, totalCount, request.PageIndex, request.PageSize);
                },
                TimeSpan.FromMinutes(2)
            );

            return ApiResponse<PagedResponse<TaskCloseRequestListDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskCloseRequestDetailsDto>> GetByIdAsync(int id)
        {

            var request = await _requestRepo.GetAll(r => r.Id == id)
                .Include(r => r.TaskAssignment)
                .Include(r => r.ReviewedBy).Include(r => r.RequestedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (request == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);
            var dto = _mapper.Map<TaskCloseRequestDetailsDto>(request);
            return ApiResponse<TaskCloseRequestDetailsDto>.Ok(dto);
        }

        public async Task<ApiResponse<TaskCloseRequestDetailsDto>> AddAsync(TaskCloseRequestAddDto dto, int taskId, int employeeId)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var assignment = await _taskAssignmentRepo.GetAll(a => a.TaskId == taskId && a.EmployeeId == employeeId && a.IsActive)
                                                      .FirstOrDefaultAsync();
            if (assignment == null)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);

            if (assignment.IsClosed) 
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);

            var hasPendingRequest = await _requestRepo.GetAll(r =>
                    r.TaskId == taskId &&
                    r.Status == CloseRequestStatus.Pending
                )
                .AnyAsync();

            if (hasPendingRequest)
                throw new AppException(ErrorCodes.CloseRequestAlreadyPending, StatusCodes.Status409Conflict);


            var request = _mapper.Map<TaskCloseRequest>(dto);

            request.TaskAssignmentId = assignment.Id;
            request.TaskId = taskId;
            request.RequestedByEmployeeId = employeeId;
            //request.CreatedBy = employeeId;
            request.RequestedAt = DateTime.UtcNow;
            request.Status = CloseRequestStatus.Pending;
            request.ReviewedByEmployeeId = null;

            await _requestRepo.AddAsync(request);
            await _requestRepo.SaveChangesAsync();
            await _cacheInvalidator.InvalidateTaskCloseRequestsAsync(taskId);
            //await _cache.RemoveAsync("taskCloseRequests:");

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


            await _eventDispatcher.PublishAsync(
                new TaskCloseRequestEvent(request.Id, taskId, employeeName, assignedEmployeeIds, task.Title)
            );


            var savedRequest = await _requestRepo.GetAll(r => r.Id == request.Id)
                                                 .Include(r => r.RequestedBy)
                                                .Include(r => r.TaskAssignment)
                                                .ThenInclude(a => a.Employee)
                                                .Include(r => r.TaskAssignment)
                                                .ThenInclude(a => a.Task)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();

            var resultDto = _mapper.Map<TaskCloseRequestDetailsDto>(savedRequest);

            return ApiResponse<TaskCloseRequestDetailsDto>.Ok(resultDto, "Close request added successfully");
        }


        public async Task<ApiResponse<TaskCloseRequestDetailsDto>> ReviewAsync(int id,CloseRequestStatus status,int reviewerId)
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

            if (task.Status == WorkTaskStatus.Closed)
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);

            if (request.Status != CloseRequestStatus.Pending)
                throw new AppException(ErrorCodes.AlreadyReviewed, StatusCodes.Status400BadRequest);

            var taskAssignments = await _taskAssignmentRepo
                    .GetAll(a => a.TaskId == task.Id && a.IsActive)
                    .ToListAsync();

            var assignedEmployeeIds = taskAssignments
                    .Select(a => a.EmployeeId)
                    .ToList();

            if (status == CloseRequestStatus.Approved)
            {

                var percent = await _AchievementRepo.GetAll(p =>p.TaskId == task.Id)          
        .OrderByDescending(p => p.CreatedDate)
        .FirstOrDefaultAsync();

                if (percent == null)
                    throw new AppException(ErrorCodes.TaskHasNoPercentage, StatusCodes.Status400BadRequest);

                request.Status = CloseRequestStatus.Approved;

                task.Status = WorkTaskStatus.Closed;
                task.ClosedAt = DateTime.UtcNow;
                task.ClosedByUserId = reviewerId;
                task.CloseReason = CloseReason.Manual;

                

                foreach (var assignment in taskAssignments)
                    assignment.IsClosed = true;

                

                if (task.AssignedByEmployeeId.HasValue &&!assignedEmployeeIds.Contains(task.AssignedByEmployeeId.Value))
                {
                    assignedEmployeeIds.Add(task.AssignedByEmployeeId.Value);
                }

                assignedEmployeeIds.Remove(reviewerId);

                await _eventDispatcher.PublishAsync(
                    new TaskCloseApproveEvent(
                        request.Id,
                        task.Id,
                        task.Title,
                        assignedEmployeeIds
                    )
                );
            }
            else
            {
                request.Status = CloseRequestStatus.Rejected;
            }

            request.ReviewedByEmployeeId = reviewerId;
            request.ReviewedAt = DateTime.UtcNow;

            await _requestRepo.SaveChangesAsync();
            await _cacheInvalidator.InvalidateTaskCloseRequestsAsync(request.TaskId);
            await _cacheInvalidator.InvalidateTasksAsync(task.CompanyId);
            if (request.Status == CloseRequestStatus.Approved)
            {
                await _cacheInvalidator.InvalidateDashboardAsync(task.CompanyId);
                foreach(var empId in assignedEmployeeIds)
                {
                    await _cacheInvalidator.InvalidateEmployeeDashboardAsync(empId);
                }
            }

            //await _cache.RemoveAsync("taskCloseRequests:");

            var updatedRequest = await _requestRepo
                .GetAll(r => r.Id == id)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReviewedBy)
                .Include(r => r.TaskAssignment)
                .ThenInclude(a => a.Employee)
                .Include(r => r.TaskAssignment)
                .ThenInclude(a => a.Task)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var resultDto = _mapper.Map<TaskCloseRequestDetailsDto>(updatedRequest);
            resultDto.TaskTitle = updatedRequest.TaskAssignment.Task?.Title;

            return ApiResponse<TaskCloseRequestDetailsDto>
                .Ok(resultDto, "Request reviewed successfully");
        }

    }

}
