using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Exercises;

public record ExerciseDetailResponse(
    Guid Id,
    string Name,
    MuscleGroup MuscleGroup,
    Equipment Equipment,
    Difficulty Difficulty,
    string? ImageUrl,
    string InstructionText,
    DateTime CreatedAt,
    DateTime UpdatedAt);
