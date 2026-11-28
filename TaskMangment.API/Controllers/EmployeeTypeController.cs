using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeTypeController : BaseController
    {
        private readonly IEmployeeTypeService _service;

        public EmployeeTypeController(IEmployeeTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EmployeeTypeRequest request)
        {
            var result = await _service.GetAllAsync(request);
            if (!result.Success)
                return Fail(result.Message!);
            return Success(result.Data);
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup()
        {
            var result = await _service.GetLookupAsync();
            return Success(result.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost]
        [PermissionAuthorize(PermissionCodes.CreateEmployeeType)]
        public async Task<IActionResult> Add([FromBody] EmployeeTypeAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            return Success(result.Data, "Employee type added successfully");
        }

        [HttpPut("{id:int}")]
        [PermissionAuthorize(PermissionCodes.UpdateEmployeeType)]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeTypeAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Employee type updated successfully");
        }

        [HttpDelete("{id:int}")]
        [PermissionAuthorize(PermissionCodes.DeleteEmployeeType)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Success(true, "Employee type deleted successfully");
        }
    }
}
