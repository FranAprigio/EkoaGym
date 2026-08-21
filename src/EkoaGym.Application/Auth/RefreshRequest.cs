using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Auth;

public record RefreshRequest([Required] string RefreshToken);
