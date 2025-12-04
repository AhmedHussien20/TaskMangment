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
    public interface ITaskCloseRequestService
    {
        Task<ApiResponse<PagedResponse<TaskCloseRequestListDto>>> GetAllAsync(TaskCloseRequestRequest request);
        Task<ApiResponse<bool>> AddAsync(TaskCloseRequestAddDto dto, int taskAssignmentId, int employeeId);
        Task<ApiResponse<TaskCloseRequestDetailsDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> ReviewAsync(int id, bool approved, int reviewerId);
    }
}
