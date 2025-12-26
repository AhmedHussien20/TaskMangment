using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskMangment.Application.Authorization;
using TaskMangment.Application.Interfaces;

namespace TaskMangment.API.Filters
{
    public class RoleAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IRoleService _roleService;

        public RoleAuthorizationFilter(IRoleService roleService)
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

            //Get the user Role here 
            //var userRoles = _roleService.

            //if(!userRoles.Any(x=>  attr.role.contain(x)))
            //{context.result = new ForbidResult()}
        }
    }
}
