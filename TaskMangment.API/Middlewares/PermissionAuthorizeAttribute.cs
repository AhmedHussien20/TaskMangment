using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Middlewares
{
    public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAuthorizeAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();

            var employeeId = int.Parse(context.HttpContext.User.FindFirst("EmployeeId").Value);

            var hasPermission = await permissionService.UserHasPermissionAsync(employeeId, _permission);

            if (!hasPermission)
            {
                context.Result = new JsonResult(new
                {
                    error = true,
                    message = "Not Allowed (No Permission)"
                })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }

}
