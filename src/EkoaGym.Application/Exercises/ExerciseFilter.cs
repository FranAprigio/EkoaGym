using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Exercises;

public record ExerciseFilter(
    MuscleGroup? MuscleGroup,
    Equipment? Equipment,
    Difficulty? Difficulty,
    string? SearchText,
    int Page,
    int PageSize);
