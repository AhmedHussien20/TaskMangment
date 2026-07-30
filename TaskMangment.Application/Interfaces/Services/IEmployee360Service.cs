using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IEmployee360Service
    {
        Task<ApiResponse<Employee360Dto>> Get360Async(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null);

        Task<ApiResponse<Employee360AccessDto>> GetAccessAsync(int actorId, int employeeId);

        Task<ApiResponse<Employee360PerformanceDto>> GetPerformanceAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null);

        Task<ApiResponse<Employee360DiscountsDto>> GetDiscountsAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null);

        Task<ApiResponse<Employee360LeaveDto>> GetLeaveAsync(
            int actorId, int employeeId, Employee360DateRangeRequest? range = null);

        Task<ApiResponse<PagedResponse<Employee360EmailItemDto>>> GetEmailsAsync(
            int actorId, int employeeId, Employee360PagedRequest request);

        Task<ApiResponse<PagedResponse<Employee360NotificationItemDto>>> GetNotificationsAsync(
            int actorId, int employeeId, Employee360PagedRequest request);

        Task<ApiResponse<PagedResponse<Employee360CommentItemDto>>> GetCommentsAsync(
            int actorId, int employeeId, Employee360PagedRequest request);

        Task<ApiResponse<PagedResponse<EmployeeTimelineItemDto>>> GetTimelineAsync(
            int actorId, int employeeId, EmployeeTimelineRequest request);

        /// <summary>
        /// Paged task list using the same assignment rules as Employee 360 KPI cards.
        /// </summary>
        Task<ApiResponse<PagedResponse<Employee360KpiTaskItemDto>>> GetKpiTasksAsync(
            int actorId, int employeeId, Employee360KpiTasksRequest request);
    }
}
