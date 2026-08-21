using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Auth;

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);
