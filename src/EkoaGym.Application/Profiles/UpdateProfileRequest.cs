using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;
using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Profiles;

public record UpdateProfileRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Name,
    [property: Required, NotInFuture] DateOnly DateOfBirth,
    [property: Required] Sex Sex,
    [property: Range(50, 300)] int HeightCm,
    [property: Range(20, 400)] decimal CurrentWeightKg,
    [property: Required] ActivityLevel ActivityLevel);
