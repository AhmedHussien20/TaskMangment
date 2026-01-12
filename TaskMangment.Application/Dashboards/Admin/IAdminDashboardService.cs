
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Admin
{
    public interface IAdminDashboardService
    {
        Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId, PeriodDto period);
        Task<ApiResponse<List<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId, PeriodDto period);
        Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId, PeriodDto period);
        Task<ApiResponse<List<PendingCloseRequestTaskDto>>>GetPendingCloseRequestsAsync(int companyId, PeriodDto period);

        Task<ApiResponse<List<TaskStatusDto>>> GetTasksByStatusAsync(int companyId, string status, PeriodDto period);
        Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(int companyId, PeriodDto period);

    }
}
