using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Company;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ApiResponse<PagedResponse<CompanyGetDto>>> GetAllAsync(CompanyRequest request);
        Task<ApiResponse<CompanyGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<CompanyGetDto>> AddAsync(CompanyAddEditDto dto);
        Task<ApiResponse<CompanyGetDto>> UpdateAsync(int id, CompanyAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
