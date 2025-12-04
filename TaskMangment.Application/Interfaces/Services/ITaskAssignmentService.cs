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
    public interface ITaskAssignmentService
    {
        Task<ApiResponse<PagedResponse<TaskAssignmentGetDto>>> GetAllAsync(TaskAssignmentRequest request);
        Task<ApiResponse<TaskAssignmentGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(TaskAssignmentAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, TaskAssignmentAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

}
