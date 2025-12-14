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
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IRepository<TaskComment> _commentRepo;
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskCommentService(
            IRepository<TaskComment> commentRepo,
            IRepository<Attachment> attachmentRepo,
            IMapper mapper,
            ICachingService cache,
            IRepository<WorkTask> taskRepo)
        {
            _commentRepo = commentRepo;
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _cache = cache;
            _taskRepo = taskRepo;
        }

        public async Task<ApiResponse<PagedResponse<TaskCommentGetDto>>> GetAllAsync(TaskCommentRequest request)
        {
            string cacheKey = $"taskComments:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<TaskCommentGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<TaskCommentGetDto>>.Ok(cached);
            }

            var query = _commentRepo.GetAll()
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
                dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.CommentId == comment.Id);
            }

            var response = new PagedResponse<TaskCommentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(8));

            return ApiResponse<PagedResponse<TaskCommentGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<TaskCommentGetDto>> GetByIdAsync(int id)
        {
            var comment = await _commentRepo.GetAll(c => c.Id == id)
                .Include(c => c.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (comment == null)
                return ApiResponse<TaskCommentGetDto>.Fail("Comment not found");

            var dto = _mapper.Map<TaskCommentGetDto>(comment);

            dto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.CommentId == id);

            return ApiResponse<TaskCommentGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<TaskCommentGetDto>> AddAsync(int taskId, int employeeId, TaskCommentAddEditDto dto)
        {
            var task = await _taskRepo.GetByIDAsync(taskId);
            if (task == null)
                return ApiResponse<TaskCommentGetDto>.Fail("Task not found", StatusCode.NotFound);
            var comment = _mapper.Map<TaskComment>(dto);
            comment.TaskId = taskId;
            comment.EmployeeId = employeeId;
            comment.CreatedDate = DateTime.UtcNow;

            await _commentRepo.AddAsync(comment);
            await _commentRepo.SaveChangesAsync();
            if (dto.File != null)
            {
                var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var filePath = Path.Combine("wwwroot/uploads/comments", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                var attachment = new Attachment
                {
                    FileName = dto.File.FileName,
                    FilePath = filePath,
                    Size = dto.File.Length,
                    CommentId = comment.Id,
                    UploadedBy = employeeId,
                    CreatedBy = employeeId,
                    ContentType = dto.File.ContentType,
                    UploadedAt = DateTime.UtcNow,
                    TaskId = taskId

                };

                await _attachmentRepo.AddAsync(attachment);
                await _attachmentRepo.SaveChangesAsync();
            }
            await _cache.RemoveAsync("taskComments:");
            var commentDto = _mapper.Map<TaskCommentGetDto>(comment);
            commentDto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.CommentId == comment.Id);

            return ApiResponse<TaskCommentGetDto>.Ok(commentDto, "Comment added successfully");
        }


        public async Task<ApiResponse<TaskCommentGetDto>> UpdateAsync(int id, TaskCommentAddEditDto dto)
        {
            var comment = await _commentRepo.GetAll(c => c.Id == id)
                                             .Include(c => c.Employee)
                                             .FirstOrDefaultAsync();

            if (comment == null)
                return ApiResponse<TaskCommentGetDto>.Fail("Comment not found");

            comment.CommentText = dto.CommentText;
            await _commentRepo.SaveChangesAsync();

            var commentDto = _mapper.Map<TaskCommentGetDto>(comment);

            commentDto.AttachmentCount = await _attachmentRepo.CountAsync(a => a.CommentId == comment.Id);

            return ApiResponse<TaskCommentGetDto>.Ok(commentDto, "Comment updated successfully");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var comment = await _commentRepo.GetByIDAsync(id);
            if (comment == null)
                return ApiResponse<bool>.Fail("Comment not found");

            _commentRepo.SoftDelete(comment);
            await _commentRepo.SaveChangesAsync();
            await _cache.RemoveAsync("taskComments:");

            return ApiResponse<bool>.Ok(true, "Comment deleted");
        }
    }
}
