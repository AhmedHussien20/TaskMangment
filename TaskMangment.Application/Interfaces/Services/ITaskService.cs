using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.ReportDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ITaskService
    {
        Task<ApiResponse<PagedResponse<TaskGetDto>>> GetAllAsync(TaskRequest request, int CompanyId, string role, int employeeId);
        Task<ApiResponse<TaskGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<TaskGetDto>> AddAsync(TaskAddEditDto dto, int createdUser, int companyId);
        Task<ApiResponse<TaskGetDto>> UpdateAsync(int id, TaskAddEditDto dto, int modifierUser);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<List<TaskAssignmentDto>>> GetAssignedEmployeesAsync(int taskId);
        Task<List<TaskReportDto>> GetTasksForReportAsync(int? assignedUserId = null,int? status = null,DateTime? fromDate = null,DateTime? toDate = null);
        Task<ApiResponse<TaskRequestsDto>> GetTaskRequestsAsync(int taskId);
        Task<ApiResponse<TaskActivitySummaryDTO>> GetTaskActivitySummaryAsync(int taskId);





    }
}
