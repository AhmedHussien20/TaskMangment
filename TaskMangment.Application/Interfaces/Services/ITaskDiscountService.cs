using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.ApiRequests.Task;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs.TaskDTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ITaskDiscountService
    {
        Task<ApiResponse<PagedResponse<DiscountGetDto>>> GetAllAsync(TaskDiscountRequest request);
        Task<ApiResponse<DiscountGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<DiscountGetDto>> AddAsync(int createdByEmployeeId,int TaskID, DiscountAddEditDto dto);
        Task<ApiResponse<DiscountGetDto>> UpdateAsync(int id, int TaskID, DiscountAddEditDto dto, int ModifiedByEmployeeId );
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
