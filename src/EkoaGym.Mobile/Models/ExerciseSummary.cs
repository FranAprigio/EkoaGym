namespace EkoaGym.Mobile.Models;

public record ExerciseSummary(
    Guid Id,
    string Name,
    string MuscleGroup,
    string Equipment,
    string Difficulty,
    string? ImageUrl,
    string? GifUrl);
