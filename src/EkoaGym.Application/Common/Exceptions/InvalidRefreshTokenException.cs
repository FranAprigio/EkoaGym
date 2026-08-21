namespace EkoaGym.Application.Common.Exceptions;

public class InvalidRefreshTokenException() : Exception("Refresh token is invalid, expired, or has been revoked.");
