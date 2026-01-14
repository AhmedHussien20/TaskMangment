using System.Globalization;
using Microsoft.Extensions.Localization;
using TaskMangment.API.Middlewares;
using TaskMangment.Application.Common.Errors;
using TaskMangment.Application.Common.Exceptions;
using TaskMangment.Application.Responses;
using TaskMangment.Utilities.Localization.Resources;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<Errors> _L;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IStringLocalizer<Errors> localizer,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _L = localizer;
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

            await WriteError(context, ex.ErrorCode, (int)ex.StatusCode);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access");

            await WriteError(
                context,
                ErrorCodes.Unauthorized,
                StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await WriteError(
                context,
                ErrorCodes.SaveFailed,
                StatusCodes.Status500InternalServerError);
        }
    }

    private async Task WriteError(HttpContext context, string errorCode,int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Error = true,
            ErrorCode = errorCode,
            Message = _L[errorCode],
            Data = null
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
