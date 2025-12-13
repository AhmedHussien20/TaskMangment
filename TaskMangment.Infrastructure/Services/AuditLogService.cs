using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.AuditLogs;
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
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditRepo;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICachingService _cache;
        private readonly IMapper _mapper;



        public AuditLogService(
            IRepository<AuditLog> auditRepo,
            ICurrentUserService currentUserService,
            IMapper mapper,
            ICachingService cache)
        {
            _auditRepo = auditRepo;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _cache = cache;
        }


        public async Task LogAsync(string entityName, int? entityId, string action, string details = null)
        {
            try
            {
                var audit = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    ChangedBy = _currentUserService.UserId,
                    ChangedAt = DateTime.UtcNow,
                    Details = details
                };

                await _auditRepo.AddAsync(audit);
                await _auditRepo.SaveChangesAsync();
                await _cache.RemoveAsync("auditLogs:");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Audit log error: {ex.Message}");
            }
        }


        public async Task<ApiResponse<PagedResponse<AuditLogDTO>>> GetAllAsync(AuditLogRequest request)
        {
            string cacheKey = $"auditLogs:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.SortDirection}:{request.searchKey}";

            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<AuditLogDTO>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<AuditLogDTO>>.Ok(cached);
            }

            var query = _auditRepo.GetAll().ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<AuditLogDTO>>(list);

            var response = new PagedResponse<AuditLogDTO>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<AuditLogDTO>>.Ok(response);
        }


        public async Task<ApiResponse<AuditLogDTO>> GetByIdAsync(int id)
        {
            var log = await _auditRepo.GetAll(x => x.Id == id).AsNoTracking().FirstOrDefaultAsync();

            if (log == null)
                return ApiResponse<AuditLogDTO>.Fail("Audit log not found");

            var dto = _mapper.Map<AuditLogDTO>(log);

            return ApiResponse<AuditLogDTO>.Ok(dto);
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var auditLog = await _auditRepo.GetByIDAsync(id);
            if (auditLog == null)
                return ApiResponse<bool>.Fail("audit log not found");

            _auditRepo.SoftDelete(auditLog);
            await _auditRepo.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "audit log deleted successfully");
        }

    }
}

