using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class RoleAssignmentController : BaseController
    {
        private readonly IRoleAssignmentService _service;
        public RoleAssignmentController(IRoleAssignmentService service)
        {
            _service = service;
        }

        [HttpGet("{roleId}/assigned-employees")]
        public async Task<IActionResult> GetAssignedEmployees(int roleId, [FromQuery] RoleAssignmentReguest request)
        {
            var result = await _service.GetAssignedEmployeesPagedAsync(roleId,request);
            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data);
        }

        [HttpPost("bulk-assign-employees")]
        public async Task<IActionResult> BulkAssignEmployees(int roleId,[FromBody] RoleWithManyEmployeeAssignDto dto)
        {
            var result = await _service.AssignEmployeesToRoleAsync(roleId,dto);
            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Employees assigned successfully");
        }
    }
}
