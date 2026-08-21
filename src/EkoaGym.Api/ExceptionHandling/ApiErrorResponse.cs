namespace EkoaGym.Api.ExceptionHandling;

// Shape of all error responses returned by the API.
public record ApiErrorResponse(string Code, string Message);
