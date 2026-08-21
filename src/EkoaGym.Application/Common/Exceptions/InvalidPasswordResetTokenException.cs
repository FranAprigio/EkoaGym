namespace EkoaGym.Application.Common.Exceptions;

public class InvalidPasswordResetTokenException() : Exception("This password reset link is invalid or has expired.");
