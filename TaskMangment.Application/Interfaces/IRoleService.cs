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
        Task<ApiResponse<bool>> AssignPermissionsAsync(int roleId, List<int> permissionIds);
        Task<ApiResponse<bool>> AssignRoleToEmployeeAsync(int employeeId, int roleId);
        Task<ApiResponse<List<RoleGetDto>>> GetRolesAsync(int companyId);
    }

}
