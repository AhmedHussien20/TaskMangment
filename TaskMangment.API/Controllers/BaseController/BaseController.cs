using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.API.Filters;
using TaskMangment.Application.Interfaces.IRepository;
using TaskMangment.Application.Responses;

namespace TaskMangment.API.Controllers
{
    [ServiceFilter(typeof(AuditLogAttribute))] 
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string? message = null)
        {
            return Ok(ApiResponse<T>.Ok(data, message));
        }

        protected IActionResult Fail(string message, int statusCode = 400)
        {
            return StatusCode(statusCode, ApiResponse<string>.Fail(message));
        }

        protected void SetCacheHeader(int seconds)
        {
            Response.Headers["Cache-Control"] = $"public, max-age={seconds}";
        }
    }



}
