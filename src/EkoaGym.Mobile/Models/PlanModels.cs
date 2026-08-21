namespace EkoaGym.Mobile.Models;

public record PlanExerciseItem(Guid Id, Guid ExerciseId, string ExerciseName, int Sets, int Reps, decimal WeightKg, int RestSeconds, int OrderIndex);

public record PlanDetail(Guid Id, string Name, IReadOnlyList<PlanExerciseItem> Exercises, DateTime CreatedAt, DateTime UpdatedAt);

public record PlanExerciseRequest(Guid ExerciseId, int Sets, int Reps, decimal WeightKg, int RestSeconds);

public record CreatePlanRequest(string Name, IReadOnlyList<PlanExerciseRequest> Exercises);

public record UpdatePlanRequest(string Name, IReadOnlyList<PlanExerciseRequest> Exercises);
