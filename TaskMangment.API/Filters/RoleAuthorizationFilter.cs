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
                .OfType<HasRoleAttribute>()
                .FirstOrDefault();

            if (attr == null)
                return;

            var userIdClaimed = context.HttpContext.User.FindFirst("UserId");
            if (userIdClaimed == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            int userId = int.Parse(userIdClaimed.Value);

            var userRoles = await _roleService.GetUserRolesAsync(userId);

            if (!userRoles.Any(r =>
                    attr.Roles.Contains(r, StringComparer.OrdinalIgnoreCase)))
            {
                //context.Result = new ForbidResult();
            
                throw new AppException(ErrorCodes.Unauthorized, StatusCodes.Status400BadRequest);
                //return;
            }
        }
    }
}
