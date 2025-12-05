using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.CalenderEvents;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ICalenderEventsService
    {
        Task<ApiResponse<PagedResponse<CalendarEventGetDto>>> GetAllAsync(CalendarEventRequest request);
        Task<ApiResponse<CalendarEventGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<CalendarEventGetDto>> AddAsync(CalendarEventAddEditDto dto, int createdByEmployeeId, int? companyId);
        Task<ApiResponse<CalendarEventGetDto>> UpdateAsync(int id, CalendarEventAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
