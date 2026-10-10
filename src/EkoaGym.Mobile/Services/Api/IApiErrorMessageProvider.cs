using System.Text.Json;

namespace EkoaGym.Mobile.Services.Api;

public sealed record ApiFieldError(string Field, string Code);

// What the server (or the transport) said went wrong, reduced to stable codes and raw arguments.
public sealed record ApiErrorInfo(
    int? StatusCode,
    string? Code,
    IReadOnlyDictionary<string, JsonElement>? Args = null,
    IReadOnlyList<ApiFieldError>? FieldErrors = null);

// Turns an error into text for the active language; ApiClient never shows the server's English message.
public interface IApiErrorMessageProvider
{
    string GetMessage(ApiErrorInfo error);
}
