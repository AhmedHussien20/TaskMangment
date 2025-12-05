using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Domain.Entities;

namespace TaskMangment.API.Filters
{
    public class AuditLogAttribute : ActionFilterAttribute
    {
        private readonly IAuditLogService _auditLogger;
        private readonly ICurrentUserService _currentUserService;

        public AuditLogAttribute(
            IAuditLogService auditLogger,
            ICurrentUserService currentUserService)
        {
            _auditLogger = auditLogger;
            _currentUserService = currentUserService;
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");

            var actionName = context.ActionDescriptor.DisplayName;

            var httpMethod = context.HttpContext.Request.Method;

            int? entityId = ExtractEntityId(context);

            var result = await next();

            var isSuccess = result.Exception == null;

            var details = new
            {
                Controller = controllerName,
                Action = actionName,
                HttpMethod = httpMethod,
                RequestUrl = context.HttpContext.Request.Path,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                StatusCode = context.HttpContext.Response.StatusCode,
                Status = isSuccess ? "Success" : "Failed",
                Timestamp = DateTime.UtcNow,
                Error = result.Exception?.Message
            };

            string actionType = GetActionType(httpMethod, actionName);

            await _auditLogger.LogAsync(
                entityName: controllerName,
                entityId: entityId,
                action: actionType,
                details: JsonSerializer.Serialize(details)
            );
        }

        private int? ExtractEntityId(ActionExecutingContext context)
        {
            if (context.RouteData.Values.TryGetValue("id", out var idValue))
            {
                if (int.TryParse(idValue?.ToString(), out int id))
                    return id;
            }

            return null;
        }

        private string GetActionType(string httpMethod, string actionName)
        {
            return httpMethod.ToUpper() switch
            {
                "POST" => "CREATE",
                "PUT" => "UPDATE",
                "PATCH" => "UPDATE",
                "DELETE" => "DELETE",
                "GET" => "VIEW",
                _ => httpMethod
            };
        }
    }
}