using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskMangment.Application.Authorization;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Interfaces;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Filters
{
    public class RoleAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IRoleAssignmentService _roleService;

        public RoleAuthorizationFilter(IRoleAssignmentService roleService)
        {
            _roleService = roleService;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var attr = context.ActionDescriptor.EndpointMetadata
                .OfType<HasMinRoleLevelAttribute>()
                .FirstOrDefault();

            if (attr == null)
                return;

            var userIdClaim = context.HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            int userId = int.Parse(userIdClaim.Value);

            var userMaxLevel = await _roleService.GetUserMaxRoleLevelAsync(userId);

            if (userMaxLevel < (int)attr.MinLevel)
            {
                throw new AppException(
                    ErrorCodes.Unauthorized,
                    StatusCodes.Status403Forbidden);
            }
        }
    }
}
