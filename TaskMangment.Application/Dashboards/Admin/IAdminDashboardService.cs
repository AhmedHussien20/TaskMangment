
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Admin
{
    public interface IAdminDashboardService
    {
        Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId,int roleLevel, int? employeeId = null,PeriodDto? period = null);
        Task<ApiResponse<List<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId, int roleLevel,int? employeeId, PeriodDto period);
        Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId, int roleLevel,int? employeeId, PeriodDto period);
        Task<ApiResponse<List<PendingCloseRequestTaskDto>>>GetPendingCloseRequestsAsync(int companyId, int roleLevel,int? employeeId, PeriodDto period);

        Task<ApiResponse<List<TaskStatusDto>>> GetTasksByStatusAsync(int companyId, string status, int roleLevel,int? employeeId, PeriodDto period);
        Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(int companyId, int roleLevel,  int? employeeId, PeriodDto period);

        Task<ApiResponse<List<DiscountGetDto>>> GetDiscountsAsync(int companyId, int roleLevel, int? employeeId, PeriodDto period);
        Task<ApiResponse<List<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(int companyId, int roleLevel,int? employeeId);
        Task<ApiResponse<List<CompletedTaskDetailDto>>> GetCompletedTasksDetailsAsync(int companyId, int roleLevel, int? employeeId, PeriodDto period);

    }
}
