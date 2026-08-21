using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Auth;

public record RegisterRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, PasswordComplexity] string Password,
    [property: Required, StringLength(200, MinimumLength = 1)] string Name);
