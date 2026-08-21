using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EkoaGym.Api.Tests;

public static class TestAuthHelper
{
    public static async Task<string> RegisterAndGetAccessTokenAsync(HttpClient client, string? email = null)
    {
        var request = new
        {
            email = email ?? $"user-{Guid.NewGuid():N}@example.com",
            password = "Password1",
            name = "Test User"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        return body!.AccessToken;
    }

    public static void AuthorizeWith(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private record RegisterResponseDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
}
