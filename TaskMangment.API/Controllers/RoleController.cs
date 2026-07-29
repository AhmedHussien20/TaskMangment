using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Job;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Responses;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly IRoleService _service;

        public RoleController(IRoleService service)
        {
            _service = service;
        }

      
        [HttpPost]
        [PermissionAuthorize(PermissionCodes.AssignRole)]
        public async Task<IActionResult> Create([FromBody] RoleAddEditDto dto)
        {
            var result = await _service.CreateAsync(dto, this.CompanyId);
            return Success(result.Data, "Role created successfully");
        }

      
        [HttpGet]
        [PermissionAuthorize(PermissionCodes.AssignRole, PermissionCodes.CreatePermission)]
        public async Task<IActionResult> GetAll([FromQuery] RoleRequest request)
        {
            var result = await _service.GetAllAsync(request, this.CompanyId);
            return Success(result.Data);
        }

         
        [HttpGet("{id:int}")]
        [PermissionAuthorize(PermissionCodes.AssignRole, PermissionCodes.CreatePermission)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id, this.CompanyId);

            return Success(result.Data);
        }

         
        [HttpPut("{id:int}")]
        [PermissionAuthorize(PermissionCodes.AssignRole)]
        public async Task<IActionResult> Update(int id, [FromBody] RoleAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto, this.CompanyId);
            return Success(result.Data, "Role updated successfully");
        }

        [HttpPut("{id:int}/notifications")]
        [PermissionAuthorize(PermissionCodes.AssignRole)]
        public async Task<IActionResult> UpdateNotifications(int id, [FromBody] RoleNotificationUpdateDto dto)
        {
            var result = await _service.UpdateNotificationsAsync(id, dto, this.CompanyId);
            return Success(result.Data, "Role notifications updated successfully");
        }

        
        [HttpDelete("{id:int}")]
        [PermissionAuthorize(PermissionCodes.AssignRole)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id, this.CompanyId);
            return Success(true, "Role deleted successfully");
        }
    }

}


