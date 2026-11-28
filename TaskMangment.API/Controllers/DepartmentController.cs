using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Filters;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DepartmentRequest request)
        {
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

            //SetCacheHeader(600); 

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost]
        [PermissionAuthorize(PermissionCodes.CreateDepartment)]
        public async Task<IActionResult> Add([FromBody] DepartmentAddEditDto dto)
        {
            var result = await _service.AddAsync(dto);
            return Success(result.Data, "Department added successfully");
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(PermissionCodes.UpdateDepartment)]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Department updated successfully");
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(PermissionCodes.DeleteDepartment)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Department deleted successfully");
        }
    }
}
