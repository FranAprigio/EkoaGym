using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EkoaGym.Mobile.Services.Api;

// Centralizes every HTTP call + error handling so pages never touch HttpClient directly.
public class ApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default) =>
        SendAsync<T>(() => httpClient.GetAsync(requestUri, ct));

    public Task<T?> PostAsync<T>(string requestUri, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(() => httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, ct));

    public Task PostAsync(string requestUri, object? body = null, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, ct));

    public Task<T?> PutAsync<T>(string requestUri, object? body = null, CancellationToken ct = default) =>
        SendAsync<T>(() => httpClient.PutAsJsonAsync(requestUri, body, JsonOptions, ct));

    public Task PutAsync(string requestUri, object? body = null, CancellationToken ct = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync(requestUri, body, JsonOptions, ct));

    public Task DeleteAsync(string requestUri, CancellationToken ct = default) =>
        SendAsync(() => httpClient.DeleteAsync(requestUri, ct));

    private async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await SendCoreAsync(send);
        if (response.Content.Headers.ContentLength is null or 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private async Task SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        using var response = await SendCoreAsync(send);
    }

    private static async Task<HttpResponseMessage> SendCoreAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Can't reach the server. Check your connection and try again.", null, "network_error");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("The request timed out. Please try again.", null, "timeout");
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        string? friendlyMessage = null;
        string? code = null;
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;

            // Custom Application exceptions use {code, message} (see GlobalExceptionHandler).
            if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
            {
                friendlyMessage = messageProp.GetString();
                code = root.TryGetProperty("code", out var codeProp) ? codeProp.GetString() : null;
            }
            // [ApiController] automatic model validation uses ValidationProblemDetails: {errors: {field: [msg,...]}}.
            else if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                var messages = errorsProp.EnumerateObject()
                    .SelectMany(field => field.Value.EnumerateArray().Select(m => m.GetString()))
                    .Where(m => !string.IsNullOrWhiteSpace(m));
                friendlyMessage = string.Join(" ", messages);
                code = "validation_error";
            }
        }
        catch
        {
            // Response body wasn't valid JSON at all — fall back to a generic message below.
        }

        friendlyMessage = string.IsNullOrWhiteSpace(friendlyMessage) ? null : friendlyMessage;
        friendlyMessage ??= response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            HttpStatusCode.NotFound => "We couldn't find what you were looking for.",
            HttpStatusCode.Conflict => "That already exists.",
            _ when (int)response.StatusCode >= 500 => "Something went wrong on our end. Please try again later.",
            _ => "Something went wrong. Please try again.",
        };

        response.Dispose();
        throw new ApiException(friendlyMessage, (int)response.StatusCode, code);
    }
}
