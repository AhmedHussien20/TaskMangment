 
using Microsoft.Extensions.Localization; 
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Interfaces.Services;
using TaskMangment.Application.Responses;
using TaskMangment.Utilities.Localization.Resources;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<Errors> _L;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IStringLocalizer<Errors> localizer,
        ILogger<ExceptionHandlingMiddleware> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _L = localizer;
        _config = config;
        _scopeFactory = scopeFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application exception");

            QueueDevEmailSafe(ex, context, ex.ErrorCode, (int)ex.StatusCode);

            await WriteError(context, ex.ErrorCode, (int)ex.StatusCode, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");
            QueueDevEmailSafe(ex, context, ErrorCodes.Unauthorized, StatusCodes.Status401Unauthorized);


            await WriteError(
                context,
                ErrorCodes.Unauthorized,
                StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            QueueDevEmailSafe(ex, context, ErrorCodes.SaveFailed, StatusCodes.Status500InternalServerError);


            await WriteError(
                context,
                ErrorCodes.SaveFailed,
                StatusCodes.Status500InternalServerError,
                ex);
        }
    }

    private async Task WriteError(HttpContext context, string errorCode, int statusCode, Exception? ex = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        string message = _L[errorCode];
        if (ex != null && !string.IsNullOrWhiteSpace(ex.Message) && ex.Message != errorCode)
            message = $"{message}: {ex.Message}";

        var response = new ApiResponse<object>
        {
            Success = false,
            Error = true,
            ErrorCode = errorCode,
            Message = message,
            Data = null
        };

        await context.Response.WriteAsJsonAsync(response);
    }


    private void QueueDevEmailSafe(Exception ex, HttpContext context, string errorCode, int statusCode)
    {
        var emails = (_config["DevAlertEmail:To"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (emails.Length == 0) return;

        var firstLine = ex.StackTrace?.Split(Environment.NewLine).FirstOrDefault();

        var subject = $"[API ERROR] {statusCode} | {errorCode} | {context.Request.Path}";
        var body = $@"
        <h3>🚨 API Error</h3>
        <p><b>StatusCode:</b> {statusCode}</p>
        <p><b>ErrorCode:</b> {errorCode}</p>
        <p><b>TraceId:</b> {context.TraceIdentifier}</p>
        <p><b>Request:</b> {context.Request.Method} {context.Request.Scheme}://{context.Request.Host}{context.Request.Path}</p>
        <p><b>Query:</b> {context.Request.QueryString}</p>
        <p><b>User:</b> {context.User?.Identity?.Name ?? "Anonymous"}</p>
        <p><b>IP:</b> {context.Connection.RemoteIpAddress}</p>
        <hr/>
        <p><b>Type:</b> {ex.GetType().FullName}</p>
        <p><b>Message:</b> {ex.Message}</p>
        <p><b>At:</b> {firstLine}</p>
        <pre style='white-space:pre-wrap'>{ex.StackTrace}</pre>
    ";

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var queue = scope.ServiceProvider.GetRequiredService<IEmailQueueService>();

                foreach (var to in emails)
                    await queue.QueueDirectAsync(to, subject, body);
            }
            catch (Exception qEx)
            {
                _logger.LogError(qEx, "Failed to queue developer email");
            }
        });
    }




}
