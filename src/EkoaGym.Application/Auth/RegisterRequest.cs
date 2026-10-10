using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, PasswordComplexity] string Password,
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [SupportedLanguage] string? PreferredLanguage = null);
