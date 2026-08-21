namespace EkoaGym.Mobile.Models;

public record ProfileSummary(
    string Name,
    DateOnly DateOfBirth,
    string Sex,
    int HeightCm,
    decimal CurrentWeightKg,
    string ActivityLevel);
