using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.ApiRequests.Discount;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IDiscountService
    {
        Task<ApiResponse<PagedResponse<DiscountListDto>>> GetAllAsync(DiscountRequest request);
        Task<ApiResponse<DiscountDetailsDto>> GetByIdAsync(int id);
        Task<ApiResponse<DiscountDetailsDto>> AddAsync(int createdByEmployeeId,int TaskID, DiscountAddEditDto dto);
        Task<ApiResponse<DiscountDetailsDto>> UpdateAsync(int id, int TaskID, DiscountAddEditDto dto, int ModifiedByEmployeeId );
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
