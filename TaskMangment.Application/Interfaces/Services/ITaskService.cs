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
    public interface ITaskService
    {
        Task<ApiResponse<PagedResponse<TaskGetDto>>> GetAllAsync(TaskRequest request);
        Task<ApiResponse<TaskGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<TaskGetDto>> AddAsync(TaskAddEditDto dto, int createdUser, int companyId);
        Task<ApiResponse<TaskGetDto>> UpdateAsync(int id, TaskAddEditDto dto, int modifierUser);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
