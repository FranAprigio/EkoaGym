using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Auth;

public record ForgotPasswordRequest([Required, EmailAddress] string Email);
