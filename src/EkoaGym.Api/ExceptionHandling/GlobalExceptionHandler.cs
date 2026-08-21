using EkoaGym.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace EkoaGym.Api.ExceptionHandling;

// Catch-all handler; maps known Application exceptions to their appropriate status code/error code.
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, response) = exception switch
        {
            EmailAlreadyInUseException => (StatusCodes.Status409Conflict, new ApiErrorResponse("email_already_in_use", exception.Message)),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, new ApiErrorResponse("invalid_credentials", exception.Message)),
            InvalidRefreshTokenException => (StatusCodes.Status401Unauthorized, new ApiErrorResponse("invalid_refresh_token", exception.Message)),
            ProfileNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("profile_not_found", exception.Message)),
            GoalNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("goal_not_found", exception.Message)),
            ExerciseNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("exercise_not_found", exception.Message)),
            PlanNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("plan_not_found", exception.Message)),
            PlanAccessDeniedException => (StatusCodes.Status403Forbidden, new ApiErrorResponse("plan_access_denied", exception.Message)),
            PlanExerciseNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("plan_exercise_not_found", exception.Message)),
            _ => (StatusCodes.Status500InternalServerError, new ApiErrorResponse("internal_error", "An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}

