using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using TaskMangment.Application.Interfaces.Services;

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

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerName = context.Controller.GetType().Name.Replace("Controller", "");
            var actionName = context.ActionDescriptor.DisplayName;
            var httpMethod = context.HttpContext.Request.Method;

            int? entityId = ExtractEntityId(context);

            ActionExecutedContext result;

            try
            {
                result = await next();
            }
            catch (Exception ex)
            {
                await LogAudit(context, controllerName, actionName, httpMethod, entityId, ex);

                throw;
            }

            await LogAudit(context, controllerName, actionName, httpMethod, entityId, null);
        }

        private async Task LogAudit(
            ActionExecutingContext context,
            string controllerName,
            string actionName,
            string httpMethod,
            int? entityId,
            Exception? exception)
        {
            // Anonymous calls (login / password reset) have no schema yet — AuthService writes login audit
            // against the db that actually found the employee. Platform actors must not write into dbo
            // company AuditLogs via this employee filter.
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
                return;

            var actor = user.FindFirst("Actor")?.Value;
            if (string.Equals(actor, "Platform", StringComparison.OrdinalIgnoreCase))
                return;

            var details = new
            {
                Controller = controllerName,
                Action = actionName,
                HttpMethod = httpMethod,
                RequestUrl = context.HttpContext.Request.Path,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                StatusCode = context.HttpContext.Response.StatusCode,
                Status = exception == null ? "Success" : "Failed",
                Timestamp = DateTime.UtcNow,
                Error = exception?.Message
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
