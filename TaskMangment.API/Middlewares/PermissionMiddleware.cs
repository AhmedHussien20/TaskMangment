using Microsoft.AspNetCore.Http;
using TaskMangment.Application.Authorization;
using TaskMangment.Application.Interfaces.Services;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IPermissionService permissionService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var attribute = endpoint.Metadata.GetMetadata<HasPermissionAttribute>();
        if (attribute == null)
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.Claims.FirstOrDefault(x => x.Type == "UserId");
        if (userIdClaim == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        int userId = int.Parse(userIdClaim.Value);

        bool hasPermission = await permissionService.UserHasPermissionAsync(userId, attribute.PermissionCode);

        if (!hasPermission)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden - You lack permission.");
            return;
        }

        await _next(context);
    }
}
