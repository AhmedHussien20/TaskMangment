using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ApiResponse<ICollection<CompanyGetDto>>> GetAllAsync();
        Task<ApiResponse<CompanyGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<bool>> AddAsync(CompanyAddEditDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, CompanyAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
