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
        Task<ApiResponse<int>> CreateRoleAsync(RoleAddDto dto);
        Task<ApiResponse<bool>> AssignPermissionsAsync(RolePermissionAssignDto dto);
        Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(AssignRoleToEmployeeDto dto);
        Task<ApiResponse<List<RoleGetDto>>> GetRolesAsync(int companyId);
    }

}
