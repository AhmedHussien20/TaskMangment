
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Admin
{
    public interface IAdminDashboardService
    {
        Task<ApiResponse<AdminDashboardDto>> GetDashboardAsync(int companyId,int roleLevel, int? employeeId = null,PeriodDto? period = null, int? branchId = null);
        Task<ApiResponse<PagedResponse<UpdatedTodayTaskDto>>> GetTodayUpdatedInProgressTasksAsync(int companyId, int roleLevel, int? employeeId, UpdatedTodayTasksRequest request,int? branchId = null);
        Task<ApiResponse<List<CompletedTodayEmployeeDto>>> GetEmployeesCompletedTasksTodayAsync(int companyId, int roleLevel,int? employeeId, PeriodDto period, int? branchId = null);
        Task<ApiResponse<List<PendingCloseRequestTaskDto>>>GetPendingCloseRequestsAsync(int companyId, int roleLevel,int? employeeId, PeriodDto period, int? branchId = null);

        Task<ApiResponse<PagedResponse<TaskStatusDto>>> GetTasksByStatusAsync(int companyId, string status,int roleLevel,int? employeeId,TasksByStatusRequest request, int? branchId = null);
        Task<ApiResponse<AdminKpisExtendedDto>> GetKpisAsync(int companyId, int roleLevel,  int? employeeId, PeriodDto period, int? branchId = null);
        Task<ApiResponse<PagedResponse<DiscountGetDto>>> GetDiscountsAsync(int companyId,int roleLevel,int? employeeId,DiscountsRequest request,int? branchId = null);        
        Task<ApiResponse<PagedResponse<HighPriorityTaskDto>>> GetHighPriorityTasksAsync(int companyId,int roleLevel,int? employeeId,TasksHighPriorityRequest request,int? branchId = null);       
        Task<ApiResponse<List<CompletedTaskDetailDto>>> GetCompletedTasksDetailsAsync(int companyId, int roleLevel, int? employeeId, PeriodDto period, int? branchId = null);
        Task<ApiResponse<List<BranchFilterDto>>> GetBranchesForFilterAsync(int companyId, int roleLevel, int employeeId);

    }
}
