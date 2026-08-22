namespace EkoaGym.Mobile.Models;

public record ExerciseDetailResponse(
    Guid Id,
    string Name,
    string MuscleGroup,
    string Equipment,
    string Difficulty,
    string? ImageUrl,
    string? GifUrl,
    string InstructionText);
