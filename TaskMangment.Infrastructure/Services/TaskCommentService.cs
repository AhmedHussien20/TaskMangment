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
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAppUnitOfWork _uow;



        public TaskCommentService(
            IRepository<TaskComment> commentRepo,
            IRepository<Attachment> attachmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IDomainEventDispatcher eventDispatcher,
            IRepository<Employee> employeeRepo,
            IBlobStorageService blobStorageService,
            IAppUnitOfWork uow
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
            _blobStorageService = blobStorageService;
            _uow = uow;

        }

        public async Task<ApiResponse<PagedResponse<TaskCommentGetDto>>> GetAllAsync(TaskCommentRequest request)
        {
            //string cacheKey = $"taskComments:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}:{request.TaskId}";

            //if (!request.BypassCache)
            //{
            //    var cached = await _cache.GetAsync<PagedResponse<TaskCommentGetDto>>(cacheKey);
            //    if (cached != null)
            //        return ApiResponse<PagedResponse<TaskCommentGetDto>>.Ok(cached);
            //}

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
            }

            var response = new PagedResponse<TaskCommentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            //await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(8));

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

        public async Task<ApiResponse<TaskCommentGetDto>> AddAsync(
     int taskId,
     int employeeId,
     int roleLevel,
     TaskCommentAddEditDto dto)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                throw new AppException(ErrorCodes.TaskNotFound, StatusCodes.Status404NotFound);

            var assignment = await _taskAssignmentRepo
                .GetAll(a => a.TaskId == taskId
                         && (a.EmployeeId == employeeId || a.CreatedBy == employeeId)
                         && a.IsActive)
                .FirstOrDefaultAsync();

            if (assignment == null && roleLevel != 100 && roleLevel != 80 && roleLevel != 70)
                throw new AppException(ErrorCodes.NotAssigned, StatusCodes.Status400BadRequest);

            if (assignment != null && assignment.IsClosed)
                throw new AppException(ErrorCodes.TaskAlreadyClosed, StatusCodes.Status400BadRequest);

            await _uow.BeginTransactionAsync();

            var uploadedBlobUrls = new List<string>();

            try
            {
                var comment = _mapper.Map<TaskComment>(dto);
                comment.TaskId = taskId;
                comment.EmployeeId = employeeId;

                if (task.Status == WorkTaskStatus.New)
                    task.Status = WorkTaskStatus.InProgress;

                await _commentRepo.AddAsync(comment);
                await _commentRepo.SaveChangesAsync();

                var files = dto.Files ?? new List<IFormFile>();
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        if (file == null || file.Length == 0) continue;

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        await using var stream = file.OpenReadStream();

                        var blobUrl = await _blobStorageService.UploadAsync(
                            stream,
                            fileName,
                            file.ContentType,
                            folder: "attachments"
                        );

                        uploadedBlobUrls.Add(blobUrl);

                        var attachment = new Attachment
                        {
                            FileName = file.FileName,
                            FilePath = blobUrl,
                            Size = file.Length,
                            UploadedBy = employeeId,
                            ContentType = file.ContentType,
                            UploadedAt = DateTime.UtcNow,

                            AttachmentType = AttachmentType.Comment,
                            ReferenceId = comment.Id,

                            BlobUrl = blobUrl,
                            BlobUploadedAt = DateTime.UtcNow,
                            IsUploadedToBlob = true
                        };

                        await _attachmentRepo.AddAsync(attachment);
                    }

                    await _attachmentRepo.SaveChangesAsync();
                }

                await _cache.RemoveAsync("taskComments:");
                await _uow.CommitAsync();

                // ==== Notifications/Event ====
                var assignedEmployeeIds = await _taskAssignmentRepo
                    .GetAll(a => a.TaskId == taskId && a.IsActive)
                    .Select(a => a.EmployeeId)
                    .ToListAsync();

                var employeeName = await _employeeRepo
                    .GetAll(e => e.Id == employeeId)
                    .Select(e => e.FullName)
                    .FirstOrDefaultAsync();

                if (task.AssignedByEmployeeId.HasValue &&
                    !assignedEmployeeIds.Contains(task.AssignedByEmployeeId.Value))
                {
                    assignedEmployeeIds.Add(task.AssignedByEmployeeId.Value);
                }

                assignedEmployeeIds.Remove(employeeId);
                assignedEmployeeIds = await _employeeRepo.GetAll(e => assignedEmployeeIds.Contains(e.Id) && e.IsActive)
                    .Select(e => e.Id)
                    .ToListAsync();

                if (assignedEmployeeIds.Any())
                {
                    await _eventDispatcher.PublishAsync(
                        new TaskCommentAddedEvent(comment.Id, taskId, employeeName, assignedEmployeeIds, task.Title)
                    );
                }

                // ==== Return DTO ====
                var savedComment = await _commentRepo
                    .GetAll(c => c.Id == comment.Id)
                    .Include(c => c.Employee)
                    .Include(c => c.Task)
                    .Include(c => c.Attachments)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                var commentDto = _mapper.Map<TaskCommentGetDto>(savedComment);

                commentDto.AttachmentCount = await _attachmentRepo.CountAsync(a =>
                    a.ReferenceId == comment.Id &&
                    a.AttachmentType == AttachmentType.Comment &&
                    !a.IsDeleted);

                commentDto.TaskTitle = savedComment?.Task?.Title;
                commentDto.EmployeeName = savedComment?.Employee?.FullName;

                return ApiResponse<TaskCommentGetDto>.Ok(commentDto, "Comment added successfully");
            }
            catch
            {
                await _uow.RollbackAsync();

                foreach (var url in uploadedBlobUrls)
                {
                    try { await _blobStorageService.DeleteAsync(url); }
                    catch {  }
                }

                throw;
            }
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

        public async Task<ApiResponse<List<AttachmentVm>>> GetCommentAttachmentsAsync(int commentId)
        {
            var list = await _attachmentRepo
                .GetAll(a =>
                    a.ReferenceId == commentId &&
                    a.AttachmentType == AttachmentType.Comment &&
                    !a.IsDeleted)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new
                {
                    a.Id,
                    a.FileName,
                    a.FilePath,
                    a.ContentType,
                    a.Size,
                    a.UploadedAt
                })
                .AsNoTracking()
                .ToListAsync();

            var result = list.Select(a =>
            {
                var sasUrl = _blobStorageService.WithSas(a.FilePath);

                return new AttachmentVm
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    Url = sasUrl,
                    UrlDownload = sasUrl,
                    ContentType = a.ContentType,
                    Size = a.Size,
                    UploadedAt = a.UploadedAt
                };
            }).ToList();

            return ApiResponse<List<AttachmentVm>>.Ok(result);
        }

        public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAttachmentAsync(int attachmentId)
        {
            var att = await _attachmentRepo
                .GetAll(a => a.Id == attachmentId && !a.IsDeleted)
                .Select(a => new { a.FileName, a.FilePath, a.ContentType })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (att == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var sasUrl = _blobStorageService.WithSas(att.FilePath);

            var http = new HttpClient();
            var resp = await http.GetAsync(sasUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!resp.IsSuccessStatusCode)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var stream = await resp.Content.ReadAsStreamAsync();
            var contentType = att.ContentType
                ?? resp.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            return (stream, contentType, att.FileName);
        }


    }
}
