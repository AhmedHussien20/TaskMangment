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
        Task<ApiResponse<PagedResponse<DiscountListDto>>> GetAllAsync(TaskDiscountRequest request);
        Task<ApiResponse<DiscountListDto>> GetByIdAsync(int id);
        Task<ApiResponse<DiscountListDto>> AddAsync(int createdByEmployeeId,int TaskID, DiscountAddEditDto dto);
        Task<ApiResponse<DiscountListDto>> UpdateAsync(int id, int TaskID, DiscountAddEditDto dto, int ModifiedByEmployeeId );
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
