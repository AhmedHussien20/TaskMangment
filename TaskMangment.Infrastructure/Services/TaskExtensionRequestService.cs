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
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class TaskExtensionRequestService : ITaskExtensionRequestsService
    {
        private readonly IRepository<TaskExtensionRequest> _requestRepo;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepo;
        private readonly IMapper _mapper;
        private readonly ICachingService _cache;

        public TaskExtensionRequestService(
            IRepository<TaskExtensionRequest> requestRepo,
            IRepository<TaskAssignment> taskAssignmentRepo,
            IMapper mapper,
            ICachingService cache)
        {
            _requestRepo = requestRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ApiResponse<PagedResponse<TaskExtensionRequestListDto>>> GetAllAsync(TaskExtensionRequestRequest request)
        {
            string cacheKey = $"taskExtensionRequests{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<TaskExtensionRequestListDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<TaskExtensionRequestListDto>>.Ok(cached);
            }

            var query = _requestRepo.GetAll()
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

            var dtos = _mapper.Map<ICollection<TaskExtensionRequestListDto>>(list);

            var response = new PagedResponse<TaskExtensionRequestListDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

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
                return ApiResponse<TaskExtensionRequestDetailsDto>.Fail("Request not found");

            var dto = _mapper.Map<TaskExtensionRequestDetailsDto>(request);
            return ApiResponse<TaskExtensionRequestDetailsDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddAsync(TaskExtensionRequestAddDto dto, int taskAssignmentId, int employeeId)
        {
            var assignment = await _taskAssignmentRepo.GetByIDAsync(taskAssignmentId);
            if (assignment == null)
                return ApiResponse<bool>.Fail("Task assignment not found");

            var request = _mapper.Map<TaskExtensionRequest>(dto);

            request.TaskAssignmentId = taskAssignmentId;
            request.RequestedByEmployeeId = employeeId;
            request.RequestedAt = DateTime.UtcNow;
            request.Status = ExtensionRequestStatus.Pending;

            await _requestRepo.AddAsync(request);
            await _requestRepo.SaveChangesAsync();

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("taskExtensionRequests-");

            return ApiResponse<bool>.Ok(true, "Extension request added successfully");
        }

        public async Task<ApiResponse<bool>> ReviewAsync(int id, bool approved, int reviewerId)
        {
            var request = await _requestRepo.GetByIDAsync(id);
            if (request == null)
                return ApiResponse<bool>.Fail("Request not found");

            request.Status = approved
                ? Domain.Entities.ExtensionRequestStatus.Approved
                : Domain.Entities.ExtensionRequestStatus.Rejected;

            request.ReviewedByEmployeeId = reviewerId;
            request.ReviewedAt = DateTime.UtcNow;

            await _requestRepo.SaveChangesAsync();

            // Optional: clear cache pattern
            // await _cache.RemoveByPatternAsync("taskExtensionRequests-");

            return ApiResponse<bool>.Ok(true, "Request reviewed successfully");
        }
    }
}
