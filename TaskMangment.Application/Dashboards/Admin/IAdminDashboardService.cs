
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Admin
{
    public interface IAdminDashboardService
    {
        Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId);
        Task<ApiResponse<List<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId);
        Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId);
        Task<ApiResponse<List<PendingCloseRequestTaskDto>>>GetPendingCloseRequestsAsync(int companyId);

    }
}
