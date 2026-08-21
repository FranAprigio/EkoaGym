using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
