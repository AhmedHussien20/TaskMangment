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
    public interface ITaskExtensionRequestsService
    {
        Task<ApiResponse<PagedResponse<TaskExtensionRequestListDto>>> GetAllAsync(TaskExtensionRequestRequest request);
        Task<ApiResponse<TaskExtensionRequestDetailsDto>> AddAsync(TaskExtensionRequestAddDto dto, int taskId, int employeeId);
        Task<ApiResponse<TaskExtensionRequestDetailsDto>> GetByIdAsync(int id);
        Task<ApiResponse<TaskExtensionRequestDetailsDto>> ReviewAsync(int id, TaskExtensionReviewDto dto, int reviewerId);
    }
}
