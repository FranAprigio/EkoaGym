namespace EkoaGym.Application.Sessions;

public record LogSetResponse(Guid Id, Guid ExerciseId, int SetNumber, int ActualReps, decimal ActualWeightKg, DateTime CompletedAt);
