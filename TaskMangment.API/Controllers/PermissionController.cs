using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PermissionRequest request)
        {
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

            // Optional: Cache header
            SetCacheHeader(600);

            return Success(result.Data);
        }

      
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] PermissionAddDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Success(result.Data, "Permission added successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionAddDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(result.Data, "Permission updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Permission deleted successfully");
        }
    }
}
