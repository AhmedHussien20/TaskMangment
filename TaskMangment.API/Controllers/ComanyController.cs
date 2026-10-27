using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.ApiRequests.Company;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Security;
using TaskMangment.Application.DTOs;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Infrastructure.DataContext;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _service;

        public CompanyController(ICompanyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CompanyRequest request)
        {
            var result = await _service.GetAllAsync(request);

            if (!result.Success)
                return Fail(result.Message!);

           // SetCacheHeader(600);

            return Success(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Success(result.Data);
        }

        [HttpPost]
        [PermissionAuthorize(PermissionCodes.CreateArea)]
        public async Task<IActionResult> Add([FromBody] CompanyAddEditDto dto)
        {
            // Trainee companies are provisioned only via the platform; never create a sibling
            // company row inside training from the company API.
            var schema = User.FindFirstValue("Schema");
            if (string.Equals(schema, OperationalSchema.Training, StringComparison.OrdinalIgnoreCase))
                return Fail(L[ErrorCodes.Unauthorized], StatusCodes.Status403Forbidden);

            var result = await _service.AddAsync(dto);
            return Success(true, "Company added successfully");
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(PermissionCodes.CreateArea)]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyAddEditDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Success(true, "Company updated successfully");
        }

        [HttpDelete("{id}")]
        [PermissionAuthorize(PermissionCodes.CreateArea)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return Success(true, "Company deleted successfully");
        }
    }
}
