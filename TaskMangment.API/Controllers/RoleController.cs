using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RoleAddDto dto)
        {
            dto.CompanyId = this.CompanyId; // companyId from logged user

            var result = await _service.CreateRoleAsync(dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Role created successfully");
        }

        [HttpPost("assign-permissions")]
        public async Task<IActionResult> AssignPermissions(RolePermissionAssignDto dto)
        {
            var result = await _service.AssignPermissionsAsync(dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Permissions assigned successfully");
        }

        [HttpPost("assign-to-employee")]
        public async Task<IActionResult> AssignRoleToEmployee(AssignRoleToEmployeeDto dto)
        {
            var result = await _service.AssignRoleToEmployeeAsync(dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Role assigned to employee successfully");
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _service.GetRolesAsync(this.CompanyId);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data);
        }
    }
}

