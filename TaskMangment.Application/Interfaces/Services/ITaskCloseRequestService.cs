using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;
using TaskMangment.Domain.Entities;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ITaskCloseRequestService
    {
        Task<ApiResponse<PagedResponse<TaskCloseRequestListDto>>> GetAllAsync(TaskCloseRequestRequest request);
        Task<ApiResponse<TaskCloseRequestDetailsDto>> AddAsync(TaskCloseRequestAddDto dto, int tasktId, int employeeId);
        Task<ApiResponse<TaskCloseRequestDetailsDto>> GetByIdAsync(int id);
        Task<ApiResponse<TaskCloseRequestDetailsDto>> ReviewAsync(int id, CloseRequestStatus status, int reviewerId);
    }
}
