namespace EkoaGym.Application.Plans;

public record PlanExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int Sets,
    int Reps,
    decimal WeightKg,
    int RestSeconds,
    int OrderIndex);
