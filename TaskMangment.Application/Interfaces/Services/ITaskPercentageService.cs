using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ITaskPercentageService
    {
        Task<ApiResponse<PagedResponse<TaskPercentageGetDto>>> GetAllAsync(TaskPercentRequest request);
        Task<ApiResponse<TaskPercentageGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<TaskPercentageGetDto>> AddAsync(int taskId, int employeeId, string role , TaskPercentageAddEditDto dto);
        Task<ApiResponse<TaskPercentageGetDto>> UpdateAsync(int id, TaskPercentageAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
