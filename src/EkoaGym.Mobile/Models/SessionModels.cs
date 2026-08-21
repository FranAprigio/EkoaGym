namespace EkoaGym.Mobile.Models;

public record StartSessionResult(Guid SessionId, Guid PlanId, DateTime StartedAt, IReadOnlyList<PlanExerciseItem> Exercises);

public record LogSetRequest(Guid ExerciseId, int SetNumber, int ActualReps, decimal ActualWeightKg);

public record LogSetResult(Guid Id, Guid ExerciseId, int SetNumber, int ActualReps, decimal ActualWeightKg, DateTime CompletedAt);

public record CompleteSessionResult(Guid Id, DateTime StartedAt, DateTime EndedAt);
