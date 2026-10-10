using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EkoaGym.Mobile.Services.Api;

// Centralizes every HTTP call + error handling so pages never touch HttpClient directly.
public class ApiClient(HttpClient httpClient, IApiErrorMessageProvider errorMessages)
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

    public Task<T?> PostMultipartAsync<T>(string requestUri, MultipartFormDataContent content, CancellationToken ct = default) =>
        SendAsync<T>(() => httpClient.PostAsync(requestUri, content, ct));

    // Used for authenticated binary responses (e.g. progress photo images) that can't be loaded via a plain
    // <img src> since the browser won't attach the bearer token — callers build a data: URI from the result.
    public async Task<(byte[] Bytes, string ContentType)> GetBytesAsync(string requestUri, CancellationToken ct = default)
    {
        using var response = await SendCoreAsync(() => httpClient.GetAsync(requestUri, ct));
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        return (bytes, contentType);
    }

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

    private async Task<HttpResponseMessage> SendCoreAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            throw Fail(new ApiErrorInfo(null, "network_error"));
        }
        catch (TaskCanceledException)
        {
            throw Fail(new ApiErrorInfo(null, "timeout"));
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var statusCode = (int)response.StatusCode;
        string? code = null;
        var args = new Dictionary<string, JsonElement>();
        var fieldErrors = new List<ApiFieldError>();
        try
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var root = document.RootElement;

            // {code, message, args?, fieldErrors?} from GlobalExceptionHandler / the validation factory.
            // The server's English "message" is deliberately ignored: only the stable code is shown to the user.
            if (root.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
            {
                code = codeProp.GetString();
            }
            else if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                // [ApiController] default ValidationProblemDetails from an older server build.
                code = "validation_error";
            }

            if (root.TryGetProperty("args", out var argsProp) && argsProp.ValueKind == JsonValueKind.Object)
            {
                foreach (var arg in argsProp.EnumerateObject())
                {
                    args[arg.Name] = arg.Value.Clone();
                }
            }

            if (root.TryGetProperty("fieldErrors", out var fieldErrorsProp) && fieldErrorsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in fieldErrorsProp.EnumerateArray())
                {
                    var field = item.TryGetProperty("field", out var f) ? f.GetString() : null;
                    var fieldCode = item.TryGetProperty("code", out var c) ? c.GetString() : null;
                    if (field is not null && fieldCode is not null)
                    {
                        fieldErrors.Add(new ApiFieldError(field, fieldCode));
                    }
                }
            }
        }
        catch
        {
            // Response body wasn't valid JSON at all — the status code alone picks the message below.
        }

        response.Dispose();
        throw Fail(new ApiErrorInfo(statusCode, code, args, fieldErrors));
    }

    private ApiException Fail(ApiErrorInfo error) =>
        new(errorMessages.GetMessage(error), error.StatusCode, error.Code);
}
