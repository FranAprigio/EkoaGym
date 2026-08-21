using Microsoft.Maui.Storage;

namespace EkoaGym.Mobile.Services.Auth;

// Tokens must never be kept in plain Preferences — SecureStorage backs onto Keystore/Keychain/DPAPI.
public class SecureStorageTokenStore : ITokenStore
{
    private const string AccessTokenKey = "ekoagym.access_token";
    private const string RefreshTokenKey = "ekoagym.refresh_token";

    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshTokenKey);

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        return Task.CompletedTask;
    }
}
