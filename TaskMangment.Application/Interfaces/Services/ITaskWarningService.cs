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
    public interface ITaskWarningService
    {
        Task<ApiResponse<PagedResponse<WarningGetDto>>> GetAllAsync(WarningRequest request);
        Task<ApiResponse<WarningGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<WarningGetDto>> AddAsync(WarningAddEditDto dto, int taskId, int employeeId);
        Task<ApiResponse<WarningGetDto>> UpdateAsync(int id, WarningAddEditDto dto, int modifiedByEmployeeId);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
