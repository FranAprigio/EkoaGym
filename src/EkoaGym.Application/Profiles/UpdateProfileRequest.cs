using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;
using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Profiles;

public record UpdateProfileRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Required, NotInFuture] DateOnly DateOfBirth,
    [Required] Sex Sex,
    [Range(50, 300)] int HeightCm,
    [Range(20, 400)] decimal CurrentWeightKg,
    [Required] ActivityLevel ActivityLevel,
    [SupportedLanguage] string? PreferredLanguage = null);
