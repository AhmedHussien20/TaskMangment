using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Branch;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IBranchService
    {
        Task<ApiResponse<PagedResponse<BranchGetDto>>> GetAllAsync(BranchRequest request, int employeeId, int roleLevel, int companyId);
        Task<ApiResponse<BranchGetDto>> GetByIdAsync(int id);
        Task<ApiResponse<BranchGetDto>> AddAsync(BranchAddEditDto dto, int CampanyId);
        Task<ApiResponse<BranchGetDto>> UpdateAsync(int id, BranchAddEditDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
