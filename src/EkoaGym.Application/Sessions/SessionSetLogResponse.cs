namespace EkoaGym.Application.Sessions;

public record SessionSetLogResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int SetNumber,
    int ActualReps,
    decimal ActualWeightKg,
    DateTime CompletedAt);
