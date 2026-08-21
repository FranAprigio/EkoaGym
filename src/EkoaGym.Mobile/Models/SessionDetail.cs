namespace EkoaGym.Mobile.Models;

public record SessionSetLog(Guid Id, Guid ExerciseId, string ExerciseName, int SetNumber, int ActualReps, decimal ActualWeightKg, DateTime CompletedAt);

public record SessionDetail(Guid Id, Guid PlanId, string PlanName, DateTime StartedAt, DateTime? EndedAt, IReadOnlyList<SessionSetLog> Sets);
