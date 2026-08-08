using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using TaskMangment.Application.Interfaces.Services;

namespace TaskMangment.API.Middlewares
{
    public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;
        private readonly bool _requireAll;

        public PermissionAuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions ?? Array.Empty<string>();
            _requireAll = false;
        }

        public PermissionAuthorizeAttribute(bool requireAll, params string[] permissions)
        {
            _permissions = permissions ?? Array.Empty<string>();
            _requireAll = requireAll;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (_permissions.Length == 0)
                return;

            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Prefer JWT permission claims (fast path)
            var claimPerms = user.FindAll("permission")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (claimPerms.Count > 0)
            {
                var ok = _requireAll
                    ? _permissions.All(p => claimPerms.Contains(p))
                    : _permissions.Any(p => claimPerms.Contains(p));

                if (!ok)
                    Forbid(context);
                return;
            }

            // Fallback: DB check
            var permissionService = context.HttpContext.RequestServices.GetService<IEmployeePermissionService>();
            if (permissionService == null)
            {
                Forbid(context);
                return;
            }

            var employeeIdClaim = user.FindFirstValue("UserId")
                ?? user.FindFirstValue("EmployeeId")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(employeeIdClaim, out var employeeId) || employeeId <= 0)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            bool has;
            if (_requireAll)
            {
                has = true;
                foreach (var p in _permissions)
                {
                    if (!await permissionService.HasAsync(employeeId, p))
                    {
                        has = false;
                        break;
                    }
                }
            }
            else
            {
                has = await permissionService.HasAnyAsync(employeeId, _permissions);
            }

            if (!has)
                Forbid(context);
        }

        private static void Forbid(AuthorizationFilterContext context)
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
