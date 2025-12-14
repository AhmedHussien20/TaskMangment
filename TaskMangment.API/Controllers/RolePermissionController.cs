using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class RolePermissionController : BaseController
    {
        private readonly IRolePermissionService _service;

        public RolePermissionController(IRolePermissionService service)
        {
            _service = service;
        }
        [HttpGet("{roleId}/assigned-permissions")]
        public async Task<IActionResult> GetAssignedPermissions(int roleId, [FromQuery] RolePermissionRequest request)
        {
            var result = await _service.GetAssignedPermissionsAsync(roleId, request);
            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data);
        }

        [HttpPost("bulk-assign-permissions")]
        public async Task<IActionResult> BulkAssignEmployees(int roleId, [FromBody] RolePermissionBulkAssignDto dto)
        {
            var result = await _service.AssignPermissionsToRoleAsync(roleId,dto);
            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "permissions assigned successfully");
        }
    }
}

