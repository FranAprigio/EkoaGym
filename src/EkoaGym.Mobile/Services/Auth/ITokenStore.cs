namespace EkoaGym.Mobile.Services.Auth;

public interface ITokenStore
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task<DateTime?> GetAccessTokenExpiresAtAsync();
    Task SaveTokensAsync(string accessToken, string refreshToken, DateTime accessTokenExpiresAt);
    Task ClearAsync();
}
