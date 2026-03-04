using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Common.ApiRequests.AuditLogs;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Controllers
{
    [Route("api/[controller]")]
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogService _service;

        //public AuditLogController(IAuditLogService service)
        //{
        //    _service = service;
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAll([FromQuery] AuditLogRequest request)
        //{
        //    var result = await _service.GetAllAsync(request);

        //    if (!result.Success)
        //        return Fail(result.Message!);

        //    // Optional: Cache header
        //    SetCacheHeader(600);

        //    return Success(result.Data);
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var result = await _service.GetByIdAsync(id);

        //    if (!result.Success)
        //        return Fail(result.Message!, 404);

        //    return Success(result.Data);
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var result = await _service.DeleteAsync(id);

        //    if (!result.Success)
        //        return Fail(result.Message, 404);

        //    return Success(true, "Employee deleted successfully");
        //}

    }
}
