namespace EkoaGym.Mobile.Models;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string Name);

public record RefreshRequest(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
