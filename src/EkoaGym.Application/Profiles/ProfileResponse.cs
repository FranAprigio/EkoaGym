using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Profiles;

public record ProfileResponse(
    string Name,
    DateOnly DateOfBirth,
    Sex Sex,
    int HeightCm,
    decimal CurrentWeightKg,
    ActivityLevel ActivityLevel);
