using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Auth;

public record ResetPasswordRequest(
    [Required] string Token,
    [Required, PasswordComplexity] string NewPassword);
