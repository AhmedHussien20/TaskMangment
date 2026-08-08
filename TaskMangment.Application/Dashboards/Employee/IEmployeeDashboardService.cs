using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Employee
{
    public interface IEmployeeDashboardService
    {
        Task<ApiResponse<EmployeeDashboardDto>> GetDashboardAsync(int employeeId, PeriodDto? period = null);
        Task<ApiResponse<PagedResponse<TodayCommentTaskDto>>> GetTasksWithoutCommentsTodayAsync(BaseApiRequest request, int employeeId, int roleLevel);
        Task<ApiResponse<List<DeductionDto>>> GetDeductionsAsync(int employeeId, PeriodDto? period = null);
        Task<ApiResponse<List<WarningDto>>> GetWarningsAsync(int employeeId, PeriodDto? period = null);

        Task<ApiResponse<List<MyTaskDto>>> GetDueSoonTasksAsync(int employeeId);
        Task<ApiResponse<EmployeeDashboardKpisExtendedDto>> GetEmployeeKpisAsync(int employeeId, PeriodDto? period = null);
        Task<ApiResponse<List<CompletedTaskDetailDto>>> GetEmployeeCompletedTasksDetailsAsync(int employeeId, PeriodDto? period = null);

    }
}
