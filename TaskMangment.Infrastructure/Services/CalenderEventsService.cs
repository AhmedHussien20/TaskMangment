using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.CalenderEvents;
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
    public class CalendarEventService : ICalenderEventsService
    {
        private readonly IRepository<CalendarEvent> _eventRepo;
        private readonly ICachingService _cache;
        private readonly IMapper _mapper;

        public CalendarEventService(IRepository<CalendarEvent> eventRepo, IMapper mapper, ICachingService cache)
        {
            _eventRepo = eventRepo;
            _cache = cache;
            _mapper = mapper;
        }
       

        public async Task<ApiResponse<PagedResponse<CalendarEventGetDto>>> GetAllAsync(CalendarEventRequest request)
        {
            string cacheKey =
                $"events:{request.PageIndex}:{request.PageSize}:{request.SortColumn}:{request.searchKey}";
            request.BypassCache = true;
            if (!request.BypassCache)
            {
                var cached = await _cache.GetAsync<PagedResponse<CalendarEventGetDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse<PagedResponse<CalendarEventGetDto>>.Ok(cached);
            }

            var query = _eventRepo.GetAll()
                .Include(e => e.RelatedTask)
                .ApplySearch(request.searchKey);

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(request.SortColumn, request.SortDirection);

            var list = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<ICollection<CalendarEventGetDto>>(list);

            var response = new PagedResponse<CalendarEventGetDto>(dtos, totalCount, request.PageIndex, request.PageSize);

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

            return ApiResponse<PagedResponse<CalendarEventGetDto>>.Ok(response);
        }
        public async Task<ApiResponse<CalendarEventGetDto>> GetByIdAsync(int id)
        {
            var ev = await _eventRepo.GetAll(e => e.Id == id)
                .Include(e => e.RelatedTask)
                .Include(e => e.CreatedBy)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (ev == null)
                return ApiResponse<CalendarEventGetDto>.Fail("Event not found", StatusCode.NotFound);

            var dto = _mapper.Map<CalendarEventGetDto>(ev);

            return ApiResponse<CalendarEventGetDto>.Ok(dto);
        }


        public async Task<ApiResponse<CalendarEventGetDto>> AddAsync(CalendarEventAddEditDto dto, int createdByEmployeeId, int? companyId)
        {
            var ev = _mapper.Map<CalendarEvent>(dto);
            ev.CreatedByEmployeeId = createdByEmployeeId;
            ev.CompanyId = companyId;
            ev.CreatedDate = DateTime.UtcNow;

            await _eventRepo.AddAsync(ev);
            await _eventRepo.SaveChangesAsync();
            await _cache.RemoveAsync("events:");
            var calendarEventdto = _mapper.Map<CalendarEventGetDto>(ev);
            return ApiResponse<CalendarEventGetDto>.Ok(calendarEventdto, "Event added");
        }

        public async Task<ApiResponse<CalendarEventGetDto>> UpdateAsync(int id, CalendarEventAddEditDto dto)
        {
            var ev = await _eventRepo.GetByIDAsync(id);
            if (ev == null)
                return ApiResponse<CalendarEventGetDto>.Fail("Event not found");

            _mapper.Map(dto, ev);

            await _eventRepo.SaveChangesAsync();
            await _cache.RemoveAsync("events:");
            var calendarEventdto = _mapper.Map<CalendarEventGetDto>(ev);

            return ApiResponse<CalendarEventGetDto>.Ok(calendarEventdto, "Event updated");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var ev = await _eventRepo.GetByIDAsync(id);
            if (ev == null)
                return ApiResponse<bool>.Fail("Event not found");

            _eventRepo.SoftDelete(ev);
            await _eventRepo.SaveChangesAsync();
            await _cache.RemoveAsync("events:");


            return ApiResponse<bool>.Ok(true, "Event deleted");
        }

        
    }

}
