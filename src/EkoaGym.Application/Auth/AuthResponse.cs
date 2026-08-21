namespace EkoaGym.Application.Auth;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
