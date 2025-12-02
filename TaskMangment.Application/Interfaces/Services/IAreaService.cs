using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IAreaService
    {
        Task<ApiResponse<ICollection<AreaGetDto>>> GetAllAsync();
        Task<ApiResponse<AreaGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(AreaAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, AreaAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
