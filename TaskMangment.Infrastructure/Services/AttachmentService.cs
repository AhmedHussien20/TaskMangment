using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Attachment;
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
    public class AttachmentService : IAttachmentService
    {
        private readonly IRepository<Attachment> _attachmentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public AttachmentService(IRepository<Attachment> attachmentRepo, IMapper mapper, ICachingService cache)
        {
            _attachmentRepo = attachmentRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<AttachmentGetDto>>> GetAllAsync(AttachmentRequest request)
        {
            string safeTaskId = request.TaskId?.ToString() ?? "null";
            string safeCommentId = request.CommentId?.ToString() ?? "null";
            string safeVoucherId = request.VoucherId?.ToString() ?? "null";

            string cacheKey = $"attachments-{request.PageIndex}-{request.PageSize}-{request.SortColumn}-{request.SortDirection}-{safeTaskId}-{safeCommentId}-{safeVoucherId}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<AttachmentGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<AttachmentGetDto>>.Ok(cached);
            }

            var query = _attachmentRepo.GetAll().AsQueryable();

            if (request.TaskId.HasValue)
                query = query.Where(a => a.TaskId == request.TaskId.Value);
            if (request.CommentId.HasValue)
                query = query.Where(a => a.CommentId == request.CommentId.Value);
            if (request.VoucherId.HasValue)
                query = query.Where(a => a.VoucherId == request.VoucherId.Value);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<AttachmentGetDto>>(list);

            var response = new PagedResponse<AttachmentGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AttachmentGetDto>>.Ok(response);
        }

        public async Task<ApiResponse<AttachmentGetDto>> GetByIdAsync(int id)
        {
            var attachment = await _attachmentRepo.GetByIDAsync(id);
            if (attachment == null)
                return ApiResponse<AttachmentGetDto>.Fail("Attachment not found");

            var dto = _mapper.Map<AttachmentGetDto>(attachment);
            return ApiResponse<AttachmentGetDto>.Ok(dto);
        }

        public async Task<ApiResponse<AttachmentGetDto>> AddAsync(AttachmentAddDto dto, int uploadedby)
        {
            var attachment = _mapper.Map<Attachment>(dto);
            attachment.UploadedBy = uploadedby;

            await _attachmentRepo.AddAsync(attachment);
            await _attachmentRepo.SaveChangesAsync();

            // Optionally, invalidate attachments cache
            var AttachmentDto = _mapper.Map<AttachmentGetDto>(attachment);

            return ApiResponse<AttachmentGetDto>.Ok(AttachmentDto, "Attachment added successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var attachment = await _attachmentRepo.GetByIDAsync(id);
            if (attachment == null)
                return ApiResponse<bool>.Fail("Attachment not found");

            _attachmentRepo.SoftDelete(attachment);
            await _attachmentRepo.SaveChangesAsync();

            // Optionally, invalidate attachments cache

            return ApiResponse<bool>.Ok(true, "Attachment deleted successfully");
        }
    }
}
