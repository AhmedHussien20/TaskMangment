using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskMangment.Application.Common.ApiRequests.CalenderEvents;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Common.Interfaces;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;
using TaskMangment.Domain.Entities.Enum;
using TaskMangment.Domain.Event;
using TaskMangment.Infrastructure.Persistence.Extensions;

namespace TaskMangment.Infrastructure.Services
{
    public class CalendarEventService : ICalenderEventsService
    {
        private readonly IRepository<CalendarEvent> _eventRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly ICachingService _cache;
        private readonly IMapper _mapper;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public CalendarEventService(
            IRepository<CalendarEvent> eventRepo,
            IRepository<Employee> employeeRepo,
            IMapper mapper,
            ICachingService cache,
            IDomainEventDispatcher eventDispatcher)
        {
            _eventRepo = eventRepo;
            _employeeRepo = employeeRepo;
            _cache = cache;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<ApiResponse<PagedResponse<CalendarEventGetDto>>> GetAllAsync(
            CalendarEventRequest request,
            int currentEmployeeId,
            int roleLevel)
        {
            var companyId = await _employeeRepo.GetAll(e => e.Id == currentEmployeeId)
                .Select(e => (int?)e.CompanyId)
                .FirstOrDefaultAsync();

            // Company-wide visibility: every event belonging to the caller's company.
            IQueryable<CalendarEvent> query = _eventRepo.GetAll()
                .Include(e => e.RelatedTask)
                .Include(e => e.CreatedBy);

            if (companyId.HasValue)
            {
                query = query.Where(e =>
                    e.CompanyId == companyId
                    || (e.CreatedBy != null && e.CreatedBy.CompanyId == companyId)
                    || e.CreatedByEmployeeId == currentEmployeeId
                    || e.Public);
            }
            else
            {
                query = query.Where(e =>
                    e.CreatedByEmployeeId == currentEmployeeId || e.Public);
            }

            query = query.ApplySearch(request.searchKey);

            if (request.From.HasValue)
            {
                var from = request.From.Value.Date;
                query = query.Where(e =>
                    (e.EndDate ?? e.StartDate) >= from);
            }

            if (request.To.HasValue)
            {
                var to = request.To.Value.TimeOfDay == TimeSpan.Zero
                    ? request.To.Value.Date.AddDays(1).AddTicks(-1)
                    : request.To.Value;
                query = query.Where(e => e.StartDate <= to);
            }

            var pageSize = request.PageSize < 1 ? 100 : Math.Min(request.PageSize, 1000);
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;

            var totalCount = await query.CountAsync();

            query = query.OrderByDynamicSafe(
                string.IsNullOrWhiteSpace(request.SortColumn) ? "StartDate" : request.SortColumn,
                string.IsNullOrWhiteSpace(request.SortDirection) ? "ASC" : request.SortDirection);

            var list = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var isPrivileged = roleLevel == 100;

            var dtos = list.Select(e =>
            {
                var dto = _mapper.Map<CalendarEventGetDto>(e);
                var isOwner = e.CreatedByEmployeeId == currentEmployeeId;
                dto.CanEdit = isOwner || isPrivileged;
                dto.CanDelete = dto.CanEdit;
                return dto;
            }).ToList();

            var response = new PagedResponse<CalendarEventGetDto>(dtos, totalCount, pageIndex, pageSize);
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
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

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

            if (ev.EventType == CalendarEventType.Holiday && ev.Public)
            {
                await _eventDispatcher.PublishAsync(
                    new PublicHolidayEvent(
                        ev.Id,
                        ev.Title,
                        ev.StartDate
                    )
                );
            }
            await _cache.RemoveAsync("events:");

            var calendarEventdto = _mapper.Map<CalendarEventGetDto>(ev);
            return ApiResponse<CalendarEventGetDto>.Ok(calendarEventdto, "Event added");
        }

        public async Task<ApiResponse<CalendarEventGetDto>> UpdateAsync(int id, CalendarEventAddEditDto dto, int currentEmployeeId, int roleLevel)
        {
            var ev = await _eventRepo.GetByIDAsync(id);
            if (ev == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var isOwner = ev.CreatedByEmployeeId == currentEmployeeId;
            var isPrivileged = roleLevel == 100;

            if (!(isOwner || isPrivileged))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            _mapper.Map(dto, ev);

            await _eventRepo.SaveChangesAsync();
            await _cache.RemoveAsync("events:");
            var calendarEventdto = _mapper.Map<CalendarEventGetDto>(ev);

            return ApiResponse<CalendarEventGetDto>.Ok(calendarEventdto, "Event updated");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id, int currentEmployeeId, int roleLevel)
        {
            var ev = await _eventRepo.GetByIDAsync(id);
            if (ev == null)
                throw new AppException(ErrorCodes.NotFound, StatusCodes.Status404NotFound);

            var isOwner = ev.CreatedByEmployeeId == currentEmployeeId;
            var isPrivileged = roleLevel == 100;

            if (!(isOwner || isPrivileged))
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status403Forbidden);

            _eventRepo.SoftDelete(ev);
            await _eventRepo.SaveChangesAsync();
            await _cache.RemoveAsync("events:");

            return ApiResponse<bool>.Ok(true, "Event deleted");
        }
    }
}
