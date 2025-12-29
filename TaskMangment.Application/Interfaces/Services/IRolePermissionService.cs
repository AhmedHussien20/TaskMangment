using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Responses;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Responses;

namespace TaskMangment.Application.Interfaces.Services
{
    public interface IRolePermissionService
    {
        Task<ApiResponse<bool>> AssignPermissionsToRoleAsync(int roleId, RolePermissionBulkAssignDto dto);
        Task<ApiResponse<PagedResponse<AssignedPermissionDto>>> GetAssignedPermissionsAsync(int roleId, RolePermissionRequest request);
        Task<List<string>> GetUserPermissionsAsync(int userId);

    }
}
