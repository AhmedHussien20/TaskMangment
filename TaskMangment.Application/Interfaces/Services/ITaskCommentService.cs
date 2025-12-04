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
    public interface ITaskCommentService
    {
        Task<ApiResponse<PagedResponse<TaskCommentGetDto>>> GetAllAsync(TaskCommentRequest request);
        Task<ApiResponse<TaskCommentGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(int taskId, int employeeId, TaskCommentAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, TaskCommentAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
