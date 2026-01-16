using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using TaskMangment.API.Filters;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Responses;
using TaskMangment.Utilities.Localization.Resources;
namespace TaskMangment.API.Controllers
{
    [ServiceFilter(typeof(AuditLogAttribute))] 
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IStringLocalizer<Errors> L =>
        HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Errors>>();
        protected int CurrentUserId
        {
            get
            {
                var claim = User.FindFirstValue("UserId");
                return int.TryParse(claim, out var id) ? id : 0;
            }
        }


        protected string CurrentUserEmail =>
            User.FindFirstValue("Email") ?? throw new AppException("FullName claim missing", StatusCodes.Status401Unauthorized);

        protected string CurrentUserFullName =>
            User.FindFirstValue("FullName") ?? throw new AppException("FullName claim missing", StatusCodes.Status401Unauthorized);

        protected int CompanyId =>
            int.Parse(User.FindFirstValue("CompanyId")
                ?? throw new AppException("CompanyId claim missing", StatusCodes.Status401Unauthorized));

        protected string Role =>
                    User.FindFirstValue(ClaimTypes.Role)
                    ?? throw new AppException("Unauthorized", StatusCodes.Status401Unauthorized);
        protected int RoleLevel =>
            int.Parse(User.FindFirstValue("RoleLevelId")?? throw new AppException("Unauthorized", StatusCodes.Status401Unauthorized));


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
