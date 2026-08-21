namespace EkoaGym.Mobile.Models;

public record ProfileSetupRequest(
    string Name,
    DateOnly DateOfBirth,
    string Sex,
    int HeightCm,
    decimal CurrentWeightKg,
    string ActivityLevel);
