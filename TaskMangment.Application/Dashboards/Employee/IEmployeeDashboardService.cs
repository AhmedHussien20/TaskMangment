using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Dashboards.Employee
{
    public interface IEmployeeDashboardService
    {
        Task<ApiResponse<EmployeeDashboardDto>> GetDashboardAsync(int employeeId);
        Task<ApiResponse<List<TodayCommentTaskDto>>> GetTasksWithoutCommentsTodayAsync(int employeeId);
        Task<ApiResponse<List<DeductionDto>>> GetDeductionsAsync(int employeeId);
        Task<ApiResponse<List<WarningDto>>> GetWarningsAsync(int employeeId);

    }
}
