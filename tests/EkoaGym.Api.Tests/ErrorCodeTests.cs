using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EkoaGym.Api.Tests;

public class ErrorCodeTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        (await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync())).RootElement.Clone();

    [Fact]
    public async Task Validation_InvalidRegisterBody_ReturnsStableFieldCodes()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email = "not-an-email", password = "short", name = "A" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonAsync(response);
        Assert.Equal("validation_error", body.GetProperty("code").GetString());

        var fieldErrors = body.GetProperty("fieldErrors").EnumerateArray()
            .Select(e => (Field: e.GetProperty("field").GetString(), Code: e.GetProperty("code").GetString()))
            .ToList();
        Assert.True(fieldErrors.Contains(("email", "invalid_email")), body.ToString());
        Assert.Contains(("password", "password_too_weak"), fieldErrors);
    }

    [Fact]
    public async Task Validation_KeepsLegacyErrorsShapeWithEnglishMessages()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email = "not-an-email", password = "Password1", name = "A" });

        var body = await ReadJsonAsync(response);

        Assert.Equal("Enter a valid email address.", body.GetProperty("errors").GetProperty("email")[0].GetString());
    }

    [Fact]
    public async Task Validation_MissingRequiredField_ReturnsRequiredCode()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "someone@example.com" });

        var body = await ReadJsonAsync(response);

        Assert.Contains(body.GetProperty("fieldErrors").EnumerateArray(),
            e => e.GetProperty("field").GetString() == "password" && e.GetProperty("code").GetString() == "required");
    }

    [Fact]
    public async Task DomainError_KeepsStableCodeRegardlessOfAcceptLanguage()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/exercises/{Guid.NewGuid()}");
        request.Headers.AcceptLanguage.ParseAdd("pt-BR");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("exercise_not_found", (await ReadJsonAsync(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnsupportedLanguage_InProfileBody_ReturnsStableCode()
    {
        var token = await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client);
        TestAuthHelper.AuthorizeWith(_client, token);

        var response = await _client.PutAsJsonAsync("/api/profile", new
        {
            name = "X", dateOfBirth = "1990-01-01", sex = "Male", heightCm = 170,
            currentWeightKg = 70, activityLevel = "LightlyActive", preferredLanguage = "fr"
        });

        var body = await ReadJsonAsync(response);
        Assert.Contains(body.GetProperty("fieldErrors").EnumerateArray(),
            e => e.GetProperty("code").GetString() == "unsupported_language");
    }
}
