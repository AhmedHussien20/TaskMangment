using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using Attachment = TaskMangment.Domain.Entities.Attachment;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IRepository<TaskComment> _commentRepo;
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;
        private readonly IDomainEventDispatcher _eventDispatcher;


        public TaskCommentService(
            IRepository<TaskComment> commentRepo,
            IRepository<Attachment> attachmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IDomainEventDispatcher eventDispatcher,
            IRepository<Employee> employeeRepo
            )
        {
            _commentRepo = commentRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _cache = cache;
            _taskRepo = taskRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _eventDispatcher = eventDispatcher;
            _employeeRepo = employeeRepo;
        }

        public async Task<ApiResponse<PagedResponse<TaskCommentGetDto>>> GetAllAsync(TaskCommentRequest request)
        {
            string cacheKey = $"taskComments:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<TaskCommentGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<TaskCommentGetDto>>.Ok(cached);
            }

            var task = await _taskRepo.GetByIDAsync(request.TaskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var query = _commentRepo.GetAll(c => c.TaskId == request.TaskId)
                .Include(c => c.Employee)
                .Include(c => c.Task)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<TaskCommentGetDto>>(list);

            foreach (var dto in dtos)
            {
                var comment = list.First(c => c.Id == dto.Id);
                dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == comment.Id && a.AttachmentType == AttachmentType.Comment);
                dto.TaskTitle = comment.Task?.Title;
                dto.EmployeeName = comment.Employee?.FullName;
                // لا حاجة لـ AttachmentCount لأنه غير مرتبط بـ Comment مباشرة
            }

            var response = new PagedResponse<TaskCommentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(8));

            return ApiResponse<PagedResponse<TaskCommentGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskCommentGetDto>> GetByIdAsync(int id)
        {
            var comment = await _commentRepo.GetAll(c => c.Id == id)
                .Include(c => c.Employee)
                .Include(c => c.Task)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (comment == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status400BadRequest);

            var dto = _mapper.Map<TaskCommentGetDto>(comment);

            dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == id && a.AttachmentType==AttachmentType.Comment);

            return ApiResponse<TaskCommentGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<TaskCommentGetDto>> AddAsync( int taskId, int employeeId, TaskCommentAddEditDto dto)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);


            var assignment = await _taskAssignmentRepo
                .GetAll(a => a.TaskId == taskId && (a.EmployeeId == employeeId || a.CreatedBy == employeeId) && a.IsActive)
                .FirstOrDefaultAsync();


            if (assignment == null)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);


            var comment = _mapper.Map<TaskComment>(dto);
            comment.TaskId = taskId;
            comment.EmployeeId = employeeId;

            await _commentRepo.AddAsync(comment); 
            await _commentRepo.SaveChangesAsync();

            if (dto.File != null)
            {
                var uploadsRoot = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "comments");

                Directory.CreateDirectory(uploadsRoot);

                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                var attachment = new Attachment
                {
                    FileName = dto.File.FileName,
                    FilePath = $"uploads/comments/{fileName}",
                    Size = dto.File.Length,
                    UploadedBy = employeeId,
                    ContentType = dto.File.ContentType,
                    UploadedAt = DateTime.UtcNow,
                    AttachmentType = AttachmentType.Comment,
                    ReferenceId = comment.Id
                };

                await _attachmentRepo.AddAsync(attachment);
                await _attachmentRepo.SaveChangesAsync();
            }
            await _cache.RemoveAsync("taskComments:");

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
                new TaskCommentAddedEvent(comment.Id, taskId, employeeName, assignedEmployeeIds, task.Title)
            );




            var savedComment = await _commentRepo
                .GetAll(c => c.Id == comment.Id)
                .Include(c => c.Employee)
                .Include(c => c.Task)
                .Include(c => c.Attachments)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var commentDto = _mapper.Map<TaskCommentGetDto>(savedComment);
            commentDto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == comment.Id && a.AttachmentType==AttachmentType.Comment);
            commentDto.TaskTitle = savedComment.Task?.Title;
            commentDto.EmployeeName = savedComment.Employee?.FullName;

            return ApiResponse<TaskCommentGetDto>
                .Ok(commentDto, "Comment added successfully");
        }

        public async Task<ApiResponse<TaskCommentGetDto>> UpdateAsync(int id, TaskCommentAddEditDto dto)
        {
            var comment = await _commentRepo.GetAll(c => c.Id == id)
                                            .Include(c => c.Employee)
                                            .Include(c => c.Task)
                                            .FirstOrDefaultAsync();

            if (comment == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            comment.CommentText = dto.CommentText;
            await _commentRepo.SaveChangesAsync();

            var savedComment = await _commentRepo.GetAll(c => c.Id == comment.Id)
                                                 .Include(c => c.Employee)
                                                 .Include(c => c.Task)
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync();

            var commentDto = _mapper.Map<TaskCommentGetDto>(savedComment);
            commentDto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.ReferenceId == comment.Id && a.AttachmentType==AttachmentType.Comment);
            commentDto.TaskTitle = savedComment.Task?.Title;
            commentDto.EmployeeName = savedComment.Employee?.FullName;

            return ApiResponse<TaskCommentGetDto>.Ok(commentDto, "Comment updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var comment = await _commentRepo.GetByIDAsync(id);
            if (comment == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            _commentRepo.SoftDelete(comment);
            await _commentRepo.SaveChangesAsync();
            await _cache.RemoveAsync("taskComments:");

            return ApiResponse<bool>.Ok(true, "Comment deleted");
        }
    }
}
