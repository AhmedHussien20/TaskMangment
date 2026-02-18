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
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.ReportDTOs;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.DataContext;
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
        private readonly AppDbContext _context;
        private readonly IRepository<TaskExtensionRequest> _extensionRequestRepo;
        private readonly IRepository<TaskCloseRequest> _closeRequestRepo;
        private readonly IRepository<Warning> _warningRepo;
        private readonly IRepository<Discount> _penaltyRepo;
        private readonly IRepository<TaskComment> _commentRepo;
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IRepository<TaskPercentage> _percentRepo;
        private readonly IRepository<Notification> _notificationRepo;

        private readonly IAppUnitOfWork _uow;





        public TaskService(
             IRepository<WorkTask> taskRepo,
            IRepository<Employee> employeeRepo,
            IRepository<TaskAssignment> assignmentRepo,
            IRepository<AuditLog> audit,
            IMapper mapper, 
            ICachingService cache ,
            IDomainEventDispatcher eventDispatcher,
            IRepository<EmailQueue> emailQueueRepo,
            AppDbContext context,
            IRepository<TaskExtensionRequest> extensionRequestRepo,
           IRepository<TaskCloseRequest> closeRequestRepo,
           IRepository<Warning> warningRepo,
           IRepository<Discount> penaltyRepo,
           IRepository<TaskComment> commentRepo,
           IRepository<Attachment> attachmentRepo,
           IRepository<TaskPercentage> percentRepo,
           IRepository<Notification> notificationRepo,
           IAppUnitOfWork uow



)
        {
            _taskRepo = taskRepo;
            _employeeRepo = employeeRepo;
            _assignmentRepo = assignmentRepo;
            _audit = audit;
            _mapper = mapper;
            _cache = cache;
            _eventDispatcher = eventDispatcher;
            _emailQueueRepo = emailQueueRepo;
            _context = context;
            _closeRequestRepo = closeRequestRepo;
            _extensionRequestRepo = extensionRequestRepo;
            _warningRepo = warningRepo;
            _penaltyRepo = penaltyRepo;
            _commentRepo = commentRepo;
            _attachmentRepo = attachmentRepo;
            _percentRepo = percentRepo;
            _notificationRepo = notificationRepo;
            _uow = uow;
        }

        public async Task<ApiResponse<PagedResponse<TaskGetDto>>> GetAllAsync(TaskRequest request, int CompanyId, int roleLevel, int employeeId)
        {
            //string cacheKey =
            //    $"tasks:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{CompanyId}:{role}:{employeeId}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<TaskGetDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<TaskGetDto>>.Ok(cached);
            //}

            var query = _taskRepo.GetAll()
                .Include(t => t.CreatedBy)
                .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                .Include(t => t.AssignedBy)
                .ApplySearch(request.searchKey);

            bool requestedAdvanced =
     request.Direction.HasValue ||
     request.TargetEmployeeId.HasValue ||
     request.PriorityId.HasValue ||
     request.CreatedFrom.HasValue || request.CreatedTo.HasValue ||
     request.DueFrom.HasValue || request.DueTo.HasValue;

            if (roleLevel == 100 && requestedAdvanced)
            {
                query = query.ApplyTaskFilters(request, employeeId);
            }
            else
            {
                // 1) exclude archived by default unless explicitly requested
                if (!request.StatusId.HasValue || request.StatusId.Value != (int)WorkTaskStatus.Archived)
                {
                    query = query.Where(t => t.Status != WorkTaskStatus.Archived);
                }


                if (request.StatusId.HasValue)
                {
                    query = query.Where(t => (int)t.Status == request.StatusId.Value);
                }

                query = query.Where(t =>
                    t.Assignments.Any(a => a.EmployeeId == employeeId && a.IsActive) ||
                    t.CreatedByEmployeeId == employeeId);

                if (request.EmployeeIds != null && request.EmployeeIds.Any())
                {
                    query = query.Where(t =>
                        t.Assignments.Any(a => a.IsActive && request.EmployeeIds.Contains(a.EmployeeId)));
                }
            }


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
                    .Where(a => a.IsActive)
                    .Select(a => new TaskEmployeeAssignmentDto
                    {
                        Id = a.Employee.Id,
                        Name = a.Employee.FullName
                    })
                    .ToList();

                dto.AssignedByName = task.AssignedBy?.FullName;
                dto.CreatedByMe =
                    roleLevel == 100 ||
                    roleLevel == 80 ||
                    task.CreatedByEmployeeId == employeeId;

            }

            var summary = new TaskSummaryDto
            {
                MyTasks = await _taskRepo.CountAsync(t =>
                t.Assignments.Any(a => a.EmployeeId == employeeId && a.IsActive)),

                CreatedByMe = await _taskRepo.CountAsync(t =>
                    t.CreatedByEmployeeId == employeeId),

                InProgressTasks = await _taskRepo.CountAsync(t =>
                    t.Status == WorkTaskStatus.InProgress &&
                    t.Assignments.Any(a => a.EmployeeId == employeeId)),

                NewTasks = await _taskRepo.CountAsync(t =>
                    t.Status == WorkTaskStatus.New &&
                    t.Assignments.Any(a => a.EmployeeId == employeeId)),
                ArchiveTasks = await _taskRepo.CountAsync(t =>
                    t.Status == WorkTaskStatus.Archived &&
                    t.Assignments.Any(a => a.EmployeeId == employeeId))
            };

            var response = new PagedResponse<TaskGetDto>(dtos, totalCount, request.PageIndex, request.PageSize, summary);

           // await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<TaskGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskGetDto>> GetByIdAsync(int id, int roleLevel, int employeeId)
        {
            var task = await _taskRepo.GetAll(t => t.Id == id)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedBy)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

            var extQuery = _extensionRequestRepo.GetAll().Where(x => x.TaskId == task.Id && !x.IsDeleted && x.Status == ExtensionRequestStatus.Approved);

            var numberOfExtensions = await extQuery.CountAsync();

            DateTime? newDate = await extQuery
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => (DateTime?)x.NewDueDate)
                .FirstOrDefaultAsync();

            var dto = _mapper.Map<TaskGetDto>(task);

            dto.NumberOfExtensions = numberOfExtensions;
            dto.NewDate = newDate;


            dto.AssignEmployee = task.Assignments
                .Where(a => a.IsActive && a.Employee.IsActive)
                .Select(a => new TaskEmployeeAssignmentDto
                {
                    Id = a.Employee.Id,
                    Name = a.Employee.FullName
                })
                .ToList();

            dto.AssignedByName = task.AssignedBy?.FullName;

            dto.CreatedByMe =
    roleLevel == 100 ||
    roleLevel == 80 ||
    task.CreatedByEmployeeId == employeeId;

            return ApiResponse<TaskGetDto>.Ok(dto);
        }



        public async Task<ApiResponse<TaskGetDto>> AddAsync(TaskAddEditDto dto, int createdUser, int companyId)
        {
            await _uow.BeginTransactionAsync();

            try
            {
                
                if (!dto.IsShared)
                {
                    int? firstTaskId = null;

                    var tasks = new List<WorkTask>();

                    foreach (var empId in dto.AssignedEmployeeIds)
                    {
                        if (!await _employeeRepo.IsExistAsync(empId))
                            throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);

                        var t = _mapper.Map<WorkTask>(dto);
                        t.CreatedByEmployeeId = createdUser;
                        t.CompanyId = companyId;
                        t.AssignedByEmployeeId = createdUser;
                        t.AssignedBy = await _employeeRepo.GetByIDAsync(createdUser);

                        await _taskRepo.AddAsync(t);
                        tasks.Add(t);
                    }

                    await _taskRepo.SaveChangesAsync();

                    firstTaskId = tasks.FirstOrDefault()?.Id;

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        var assignment = new TaskAssignment
                        {
                            TaskId = tasks[i].Id,
                            EmployeeId = dto.AssignedEmployeeIds[i],
                            IsActive = true
                        };

                        await _assignmentRepo.AddAsync(assignment);
                    }

                    await _assignmentRepo.SaveChangesAsync();

                    await _cache.RemoveAsync("tasks:");

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        await _eventDispatcher.PublishAsync(
                            new TaskAssignedEvent(tasks[i].Id, tasks[i].Title, new List<int> { dto.AssignedEmployeeIds[i] })
                        );
                    }

                    var fullTask = await _taskRepo.GetAll(t => t.Id == firstTaskId.Value)
                                 .Include(t => t.CreatedBy)
                                 .Include(t => t.AssignedBy)
                                 .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync();

                    var taskDto = _mapper.Map<TaskGetDto>(fullTask);

                    await _uow.CommitAsync();
                    return ApiResponse<TaskGetDto>.Ok(taskDto, "Task added successfully");
                }

                
                var task = _mapper.Map<WorkTask>(dto);
                task.CreatedByEmployeeId = createdUser;
                task.CompanyId = companyId;
                task.AssignedByEmployeeId = createdUser;
                task.AssignedBy = await _employeeRepo.GetByIDAsync(createdUser);

                await _taskRepo.AddAsync(task);
                await _taskRepo.SaveChangesAsync();

                foreach (var empId in dto.AssignedEmployeeIds)
                {
                    if (!await _employeeRepo.IsExistAsync(empId))
                        throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);

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

                await _eventDispatcher.PublishAsync(new TaskAssignedEvent(task.Id, task.Title, dto.AssignedEmployeeIds));

                var fullTaskShared = await _taskRepo.GetAll(t => t.Id == task.Id)
                             .Include(t => t.CreatedBy)
                             .Include(t => t.AssignedBy)
                             .Include(t => t.Assignments).ThenInclude(a => a.Employee)
                             .AsNoTracking()
                             .FirstOrDefaultAsync();

                var taskDtoShared = _mapper.Map<TaskGetDto>(fullTaskShared); ;

                await _uow.CommitAsync();
                return ApiResponse<TaskGetDto>.Ok(taskDtoShared, "Task added successfully");
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<ApiResponse<TaskGetDto>> UpdateAsync(int id, TaskAddEditDto dto, int modifierUser)
        {
            var task = await _taskRepo.GetByIDAsync(id);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status400BadRequest);

            var currentStatus = task.Status;

            if (currentStatus == WorkTaskStatus.Closed || currentStatus == WorkTaskStatus.AutoClose || currentStatus == WorkTaskStatus.Archived)
            {
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);
            }

            if (dto.Status == WorkTaskStatus.Archived)
            {
                var canArchive = currentStatus == WorkTaskStatus.Closed || currentStatus == WorkTaskStatus.AutoClose;
                if (!canArchive)
                    throw new AppException(ErrorCodes.TaskMustBeClosedBeforeArchive, StatusCodes.Status400BadRequest);
            }
            _mapper.Map(dto, task);

            if (dto.Status == WorkTaskStatus.Closed)
            {
                task.ClosedAt = DateTime.UtcNow;
                task.ClosedByUserId = modifierUser;
                task.CloseReason= CloseReason.Admin;
                var assignmentsToClose = await _assignmentRepo.GetAll(a => a.TaskId == id).ToListAsync();
                foreach (var assignment in assignmentsToClose)
                {
                    assignment.IsClosed = true;
                }

            }

            if (dto.Status == WorkTaskStatus.Archived)
            {
                task.Status = WorkTaskStatus.Archived;
            }


            var existingAssignments = await _assignmentRepo
                .GetAll(a => a.TaskId == id)
                .ToListAsync();

            var newEmployeeIds = dto.AssignedEmployeeIds ?? new List<int>();

            var reactivatedEmployeeIds = new List<int>();

            var newlyAssignedEmployeeIds = new List<int>();
            var unAssignedEmployeeIds = new List<int>();


            foreach (var oldAssignment in existingAssignments)
            {
                if (!newEmployeeIds.Contains(oldAssignment.EmployeeId))
                {
                    oldAssignment.IsActive = false;
                    unAssignedEmployeeIds.Add(oldAssignment.EmployeeId);

                }
                else
                {
                    if (!oldAssignment.IsActive)
                    {
                        oldAssignment.IsActive = true;
                        reactivatedEmployeeIds.Add(oldAssignment.EmployeeId);
                    }
                }
            }

            foreach (var empId in newEmployeeIds)
            {
                var assignment = existingAssignments
                    .FirstOrDefault(a => a.EmployeeId == empId);

                if (assignment != null)
                {
                    continue;
                }
                else
                {
                    await _assignmentRepo.AddAsync(new TaskAssignment
                    {
                        TaskId = id,
                        EmployeeId = empId,
                        IsActive = true
                    });
                    newlyAssignedEmployeeIds.Add(empId);
                }
            }

            await _assignmentRepo.SaveChangesAsync();
            await _cache.RemoveAsync("tasks:");

            if (newlyAssignedEmployeeIds.Any())
            {
                await _eventDispatcher.PublishAsync(
                    new TaskAssignedEvent(task.Id, task.Title, newlyAssignedEmployeeIds)
                );
            }

            if (reactivatedEmployeeIds.Any())
            {
                await _eventDispatcher.PublishAsync(
                    new TaskAssignedEvent(task.Id, task.Title, reactivatedEmployeeIds)
                );
            }
            if (unAssignedEmployeeIds.Any())
            {
                await _eventDispatcher.PublishAsync(new TaskUnAssignedEvent(task.Id, task.Title, unAssignedEmployeeIds));
            }

            var fullTask = await _taskRepo.GetAll(t => t.Id == task.Id)
                .Include(t => t.Company)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedBy)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Employee)
                .Include(t => t.Comments)
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


            var hasDependencies =
    await _warningRepo.GetAll(w => w.TaskId == id).AnyAsync()
 || await _penaltyRepo.GetAll(d => d.TaskId == id).AnyAsync()
 || await _percentRepo.GetAll(a => a.TaskId == id).AnyAsync()
 || await _closeRequestRepo.GetAll(c => c.TaskId == id).AnyAsync()
 || await _extensionRequestRepo.GetAll(e => e.TaskId == id).AnyAsync()
 || await _commentRepo.GetAll(e => e.TaskId == id).AnyAsync();
            if (hasDependencies)
                throw new AppException(ErrorCodes.CannotDeleteTask, StatusCodes.Status400BadRequest);



            _taskRepo.SoftDelete(task);

            await _assignmentRepo.GetAll(a => a.TaskId == task.Id && !a.IsDeleted)
     .ExecuteUpdateAsync(setters => setters
         .SetProperty(a => a.IsDeleted, true)
         .SetProperty(a => a.DeletedDate, DateTime.UtcNow)
     );
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
                Status = a.IsActive ? "Active" : "Inactive",
                IsRead = false
            }).ToList();

            var employeeIds = dtos.Select(x => x.EmployeeId).Distinct().ToList();

            var notifReadMap = await _notificationRepo.GetAll(n =>
                    n.NotificationType == NotificationType.TaskAssign &&
                    n.ReferenceId == taskId &&
                    employeeIds.Contains(n.UserId))
                .Select(n => new { n.UserId, n.IsRead })
                .ToListAsync();

            var seenDict = notifReadMap
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Any(x => x.IsRead));

            foreach (var dto in dtos)
            {
                dto.IsRead = seenDict.TryGetValue(dto.EmployeeId, out var seen) && seen;
            }

            return ApiResponse<List<TaskAssignmentDto>>.Ok(dtos);


            return ApiResponse<List<TaskAssignmentDto>>.Ok(dtos);
        }


        public async Task<List<TaskReportDto>> GetTasksForReportAsync(int? assignedUserId = null, int? status = null,DateTime? fromDate = null,DateTime? toDate = null)
        {
            IQueryable<WorkTask> query = _context.Tasks.Include(t => t.Assignments) .ThenInclude(a => a.Employee);
            if (assignedUserId.HasValue)
            {
                query = query.Where(t =>
                    t.Assignments.Any(a => a.EmployeeId == assignedUserId));
            }

            if (status.HasValue)
            {
                query = query.Where(t => (int)t.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate <= toDate.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedDate)
                .Select(t => new TaskReportDto
                {
                    Title = t.Title,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    AssignedTo = t.Assignments
                        .Select(a => a.Employee.FullName)
                        .FirstOrDefault() ?? "غير مسند",
                    DueDate = t.DueDate
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ApiResponse<TaskRequestsDto>> GetTaskRequestsAsync(int taskId)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var extensionRequests = await _extensionRequestRepo.GetAll(r => r.TaskId == taskId)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReviewedBy)
                .AsNoTracking()
                .ToListAsync();

            var extensionDtos = _mapper.Map<List<TaskExtensionRequestDetailsDto>>(extensionRequests);

            var closeRequests = await _closeRequestRepo.GetAll(r => r.TaskId == taskId)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReviewedBy)
                .AsNoTracking()
                .ToListAsync();

            var closeDtos = _mapper.Map<List<TaskCloseRequestDetailsDto>>(closeRequests);

            var result = new TaskRequestsDto
            {
                TaskId = taskId,
                ExtensionRequests = extensionDtos,
                CloseRequests = closeDtos
            };

            return ApiResponse<TaskRequestsDto>.Ok(result);
        }


        public async Task<ApiResponse<TaskActivitySummaryDTO>> GetTaskActivitySummaryAsync(int taskId)
        {
            if (!await _taskRepo.IsExistAsync(taskId))
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);


            ///////////////////comment///////////
            var commentsQuery = _commentRepo.GetAll(c => c.TaskId == taskId);
            var commentsCount = await commentsQuery.CountAsync();

            TaskCommentGetDto? lastCommentDto = null;
            if (commentsCount > 0)
            {
                var lastComment = await commentsQuery
                    .Include(c => c.Employee)
                    .Include(c => c.Task)
                    .OrderByDescending(c => c.CreatedDate)
                    .FirstOrDefaultAsync();

                if (lastComment != null)
                {
                    lastCommentDto = _mapper.Map<TaskCommentGetDto>(lastComment);
                    lastCommentDto.AttachmentCount = await _attachmentRepo.CountAsync(a =>
                        a.ReferenceId == lastComment.Id &&
                        a.AttachmentType == AttachmentType.Comment);

                    lastCommentDto.EmployeeName = lastComment.Employee?.FullName;
                    lastCommentDto.TaskTitle = lastComment.Task?.Title;
                }
            }

            // ================= WARNINGS / DISCOUNTS =================
            var warningsQuery = _warningRepo.GetAll(d => d.TaskId == taskId);

            var warningsCount = await warningsQuery.CountAsync();

            WarningGetDto? lastWarningDto = null;
            if (warningsCount > 0)
            {
                var lastWarning = await warningsQuery
                    .Include(d => d.Issued)
                    .Include(d => d.IssuedBy)
                    .Include(d => d.Task)
                    .OrderByDescending(d => d.CreatedDate)
                    .FirstOrDefaultAsync();

                if (lastWarning != null)
                    lastWarningDto = _mapper.Map<WarningGetDto>(lastWarning);
            }




            var PenaltysQuery = _penaltyRepo.GetAll(d => d.TaskId == taskId);

            var PenaltysCount = await PenaltysQuery.CountAsync();

            DiscountGetDto? LastPenaltyDto = null;
            if (PenaltysCount > 0)
            {
                var LastPenalty = await PenaltysQuery
                    .Include(d => d.Employee)
                    .Include(d => d.Task)
                    .OrderByDescending(d => d.CreatedDate)
                    .FirstOrDefaultAsync();

                if (LastPenalty != null)
                    LastPenaltyDto = _mapper.Map<DiscountGetDto>(LastPenalty);
            }

            // ================= EXTENSION REQUESTS =================
            var extensionQuery = _extensionRequestRepo.GetAll(r => r.TaskId == taskId);

            var extensionCount = await extensionQuery.CountAsync();

            TaskExtensionRequestDetailsDto? lastExtensionDto = null;
            if (extensionCount > 0)
            {
                var lastExtension = await extensionQuery
                    .Include(r => r.RequestedBy)
                    .Include(r => r.ReviewedBy)
                    .Include(r => r.TaskAssignment)
                        .ThenInclude(a => a.Employee)
                    .Include(r => r.TaskAssignment)
                        .ThenInclude(a => a.Task)
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefaultAsync();

                if (lastExtension != null)
                    lastExtensionDto = _mapper.Map<TaskExtensionRequestDetailsDto>(lastExtension);
            }

            // ================= CLOSE REQUESTS =================
            var closeQuery = _closeRequestRepo.GetAll(r => r.TaskId == taskId);

            var closeCount = await closeQuery.CountAsync();

            TaskCloseRequestDetailsDto? lastCloseDto = null;
            if (closeCount > 0)
            {
                var lastClose = await closeQuery
                    .Include(r => r.RequestedBy)
                    .Include(r => r.ReviewedBy)
                    .Include(r => r.TaskAssignment)
                        .ThenInclude(a => a.Employee)
                    .Include(r => r.TaskAssignment)
                        .ThenInclude(a => a.Task)
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefaultAsync();

                if (lastClose != null)
                    lastCloseDto = _mapper.Map<TaskCloseRequestDetailsDto>(lastClose);
            }



            var percentQuery = _percentRepo.GetAll(c => c.TaskId == taskId);
            var PercentCount = await percentQuery.CountAsync();

            TaskPercentageGetDto? lastPercentDto = null;
            if (PercentCount > 0)
            {
                var lastPercent = await percentQuery
                    .Include(c => c.Employee)
                    .Include(c => c.Task)
                    .OrderByDescending(c => c.CreatedDate)
                    .FirstOrDefaultAsync();

                if (lastPercent != null)
                    lastPercentDto = _mapper.Map<TaskPercentageGetDto>(lastPercent);


            }

            // ================= RESULT =================
            var result = new TaskActivitySummaryDTO
            {
                TaskId = taskId,

                CommentsCount = commentsCount,
                LastComment = lastCommentDto,

                WarningsCount = warningsCount,
                LastWarning = lastWarningDto,

                PenaltysCount = PenaltysCount,
                LastPenalty = LastPenaltyDto,

                ExtensionRequestsCount = extensionCount,
                LastExtensionRequest = lastExtensionDto,

                CloseRequestsCount = closeCount,
                LastCloseRequest = lastCloseDto,

                PercentageCount = PercentCount,
                LastPercentage = lastPercentDto
            };

            return ApiResponse<TaskActivitySummaryDTO>.Ok(result);
        }

        public async Task<ApiResponse<bool>> ArchiveClosedTasksAsync(List<int> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);

            var tasks = await _taskRepo.GetAll(t => taskIds.Contains(t.Id))
                .ToListAsync();

            var missingIds = taskIds.Except(tasks.Select(t => t.Id)).ToList();
            if (missingIds.Any())
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var closedTasks = tasks.Where(t => t.Status == WorkTaskStatus.Closed || t.Status == WorkTaskStatus.AutoClose).ToList();
            if (!closedTasks.Any())
                return ApiResponse<bool>.Ok(true);

            foreach (var t in closedTasks)
            {
                t.Status = WorkTaskStatus.Archived;
            }

            await _taskRepo.SaveChangesAsync();
           // await _cache.RemoveAsync("tasks:");

            return ApiResponse<bool>.Ok(true);
        }


    }
}
