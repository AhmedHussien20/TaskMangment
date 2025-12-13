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
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(int userId, string permissionCode);
        Task<ApiResponse<PagedResponse<PermissionGetDto>>> GetAllAsync(PermissionRequest request);
        Task<ApiResponse<PermissionGetDto>> CreateAsync(PermissionAddDto dto);
        Task<ApiResponse<PermissionGetDto>> UpdateAsync(int id, PermissionAddDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);


    }

}
