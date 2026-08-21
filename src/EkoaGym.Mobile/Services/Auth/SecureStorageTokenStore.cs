using System.Globalization;
using Microsoft.Maui.Storage;

namespace EkoaGym.Mobile.Services.Auth;

// Tokens must never be kept in plain Preferences — SecureStorage backs onto Keystore/Keychain/DPAPI.
public class SecureStorageTokenStore : ITokenStore
{
    private const string AccessTokenKey = "ekoagym.access_token";
    private const string RefreshTokenKey = "ekoagym.refresh_token";
    private const string AccessTokenExpiresAtKey = "ekoagym.access_token_expires_at";

    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync() => SecureStorage.Default.GetAsync(RefreshTokenKey);

    public async Task<DateTime?> GetAccessTokenExpiresAtAsync()
    {
        var raw = await SecureStorage.Default.GetAsync(AccessTokenExpiresAtKey);
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt)
            ? expiresAt
            : null;
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessTokenExpiresAt)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SecureStorage.Default.SetAsync(AccessTokenExpiresAtKey, accessTokenExpiresAt.ToString("O", CultureInfo.InvariantCulture));
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(AccessTokenExpiresAtKey);
        return Task.CompletedTask;
    }
}
