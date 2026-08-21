using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Auth;

public record RefreshRequest([property: Required] string RefreshToken);
