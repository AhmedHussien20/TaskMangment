using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        protected int CurrentUserId =>
       int.Parse(User.FindFirstValue("UserId"));

        protected string CurrentUserEmail =>
            User.FindFirstValue("Email");

        protected string CurrentUserFullName =>
            User.FindFirstValue("FullName");

        protected int CompanyId =>
           int.Parse(User.FindFirstValue("CompanyId"));
        protected bool IsAuthenticated => User.Identity.IsAuthenticated;
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
