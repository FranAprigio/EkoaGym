namespace EkoaGym.Application.ExerciseLimits;

public record ExerciseLimitResponse(Guid ExerciseId, string ExerciseName, decimal MaxWeightKg);
