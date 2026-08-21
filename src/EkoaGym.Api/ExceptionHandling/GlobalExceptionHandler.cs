using Microsoft.AspNetCore.Diagnostics;

namespace EkoaGym.Api.ExceptionHandling;

// Catch-all handler; specific domain exceptions can be mapped to dedicated codes/status here later.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiErrorResponse("internal_error", "An unexpected error occurred.");
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
