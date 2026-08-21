using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EkoaGym.Mobile.Models;
using EkoaGym.Mobile.Services.Auth;

namespace EkoaGym.Mobile.Services.Api;

// Attaches the bearer token to every request and performs one silent refresh on a 401 before
// giving up, clearing stored tokens, and signalling AuthState so the app can navigate to Login.
public class AuthDelegatingHandler(
    ITokenStore tokenStore,
    AuthState authState,
    IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(accessToken))
        {
            return response;
        }

        response.Dispose();

        var newAccessToken = await TryRefreshTokenAsync(cancellationToken);
        if (newAccessToken is null)
        {
            await tokenStore.ClearAsync();
            authState.SignOut();
            return await base.SendAsync(await CloneRequestAsync(request), cancellationToken);
        }

        var retryRequest = await CloneRequestAsync(request);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task<string?> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var refreshToken = await tokenStore.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return null;
        }

        try
        {
            // Uses a plain client (no AuthDelegatingHandler attached) to avoid recursing into itself.
            using var refreshClient = httpClientFactory.CreateClient(ApiClientRegistration.RefreshClientName);
            using var response = await refreshClient.PostAsJsonAsync("api/auth/refresh", new RefreshRequest(refreshToken), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            if (auth is null)
            {
                return null;
            }

            await tokenStore.SaveTokensAsync(auth.AccessToken, auth.RefreshToken, auth.AccessTokenExpiresAt);
            return auth.AccessToken;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content is not null)
        {
            var buffer = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(buffer);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
