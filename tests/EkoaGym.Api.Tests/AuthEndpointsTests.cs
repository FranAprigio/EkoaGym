using System.Net;
using System.Net.Http.Json;

namespace EkoaGym.Api.Tests;

public class AuthEndpointsTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly ApiTestFactory _factory = factory;

    [Fact]
    public async Task Register_ValidRequest_ReturnsAccessAndRefreshTokens()
    {
        var email = $"register-{Guid.NewGuid():N}@example.com";
        var request = new { email, password = "Password1", name = "New User" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonDocumentWrapper>();
        Assert.False(string.IsNullOrWhiteSpace(body!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(body.RefreshToken));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var request = new { email, password = "Password1", name = "User" };
        await _client.PostAsJsonAsync("/api/auth/register", request);

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password1", name = "User" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsSuccessOrExpectedNotFound()
    {
        var token = await TestAuthHelper.RegisterAndGetAccessTokenAsync(_client);
        TestAuthHelper.AuthorizeWith(_client, token);

        // No profile created yet, so 404 is the expected (non-401) outcome — proves the token was accepted.
        var response = await _client.GetAsync("/api/profile");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_KnownEmail_ReturnsOkAndAllowsPasswordReset()
    {
        var email = $"forgot-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password1", name = "User" });

        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resetToken = _factory.EmailSender.GetLastResetToken(email);
        Assert.False(string.IsNullOrWhiteSpace(resetToken));

        var resetResponse = await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token = resetToken, newPassword = "NewPassword2" });

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        var loginWithOldPassword = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password1" });
        Assert.Equal(HttpStatusCode.Unauthorized, loginWithOldPassword.StatusCode);

        var loginWithNewPassword = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "NewPassword2" });
        Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturnsOk()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/forgot-password",
            new { email = $"unknown-{Guid.NewGuid():N}@example.com" });

        // Anti-enumeration: same generic response whether or not the email is registered.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token = "not-a-real-token", newPassword = "NewPassword2" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private record JsonDocumentWrapper(string AccessToken, string RefreshToken);
}
