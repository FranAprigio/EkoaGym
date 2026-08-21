namespace EkoaGym.Mobile.Services.Api;

// Thrown by ApiClient with a message already safe to show directly to the user.
public class ApiException(string userMessage, int? statusCode = null, string? errorCode = null)
    : Exception(userMessage)
{
    public int? StatusCode { get; } = statusCode;
    public string? ErrorCode { get; } = errorCode;
}
