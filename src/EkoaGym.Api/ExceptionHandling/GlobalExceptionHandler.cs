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
            InvalidPasswordResetTokenException => (StatusCodes.Status400BadRequest, new ApiErrorResponse("invalid_password_reset_token", exception.Message)),
            ProfileNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("profile_not_found", exception.Message)),
            GoalNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("goal_not_found", exception.Message)),
            ExerciseNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("exercise_not_found", exception.Message)),
            PlanNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("plan_not_found", exception.Message)),
            PlanAccessDeniedException => (StatusCodes.Status403Forbidden, new ApiErrorResponse("plan_access_denied", exception.Message)),
            PlanExerciseNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("plan_exercise_not_found", exception.Message)),
            SessionNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("session_not_found", exception.Message)),
            SessionAlreadyCompletedException => (StatusCodes.Status409Conflict, new ApiErrorResponse("session_already_completed", exception.Message)),
            ProgressPhotoNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("progress_photo_not_found", exception.Message)),
            InvalidFileUploadException upload => (StatusCodes.Status400BadRequest, new ApiErrorResponse(upload.ErrorCode, exception.Message, upload.Args)),
            WeightLimitExceededException limit => (StatusCodes.Status400BadRequest, new ApiErrorResponse("weight_limit_exceeded", exception.Message,
                new Dictionary<string, object?> { ["maxWeightKg"] = limit.MaxWeightKg })),
            SessionNotCompletedException => (StatusCodes.Status409Conflict, new ApiErrorResponse("session_not_completed", exception.Message)),
            SessionSetLogNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("session_set_log_not_found", exception.Message)),
            TargetUserNotFoundException => (StatusCodes.Status404NotFound, new ApiErrorResponse("target_user_not_found", exception.Message)),
            ReportRateLimitExceededException => (StatusCodes.Status429TooManyRequests, new ApiErrorResponse("report_rate_limit_exceeded", exception.Message)),
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

