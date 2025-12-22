using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Job;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces;

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
        public async Task<IActionResult> Create([FromBody] RoleAddEditDto dto)
        {
            var result = await _service.CreateAsync(dto, this.CompanyId);
            return Success(result.Data, "Role created successfully");
        }

      
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RoleRequest request)
        {
            var result = await _service.GetAllAsync(request, this.CompanyId);
            return Success(result.Data);
        }

         
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id, this.CompanyId);

            return Success(result.Data);
        }

         
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto, this.CompanyId);
            return Success(result.Data, "Role updated successfully");
        }

        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id, this.CompanyId);
            return Success(true, "Role deleted successfully");
        }
    }

}


