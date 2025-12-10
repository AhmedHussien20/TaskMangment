using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests.Area;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAreaService
    {
        Task<ApiResponse<PagedResponse<AreaGetDto>>> GetAllAsync(AreaRequest request,int CompanyId);
        Task<ApiResponse<AreaGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<AreaGetDto>> AddAsync(AreaAddEditDto dto, int CompanyId);
        Task<ApiResponse<AreaGetDto>> UpdateAsync(int id, AreaAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
