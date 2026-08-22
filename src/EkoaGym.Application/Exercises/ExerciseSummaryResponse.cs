using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Exercises;

public record ExerciseSummaryResponse(
    Guid Id,
    string Name,
    MuscleGroup MuscleGroup,
    Equipment Equipment,
    Difficulty Difficulty,
    string? ImageUrl,
    string? GifUrl);
