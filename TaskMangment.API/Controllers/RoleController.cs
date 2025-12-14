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
            dto.CompanyId = this.CompanyId; 

            var result = await _service.CreateRoleAsync(dto);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data, "Role created successfully");
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


