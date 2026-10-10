namespace EkoaGym.Mobile.Models;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string Name, string? PreferredLanguage = null);

public record RefreshRequest(string RefreshToken);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
