using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<PagedResponse<RoleGetDto>>> GetAllAsync(RoleRequest request, int companyId);
        Task<ApiResponse<RoleGetDto>> GetByIdAsync(int id, int companyId);
        Task<ApiResponse<int>> CreateAsync(RoleAddEditDto dto, int companyId);
        Task<ApiResponse<RoleGetDto>> UpdateAsync(int id, RoleAddEditDto dto, int companyId);
        Task<ApiResponse<bool>> DeleteAsync(int id, int companyId);

    }


}
