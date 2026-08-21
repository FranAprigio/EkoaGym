namespace EkoaGym.Application.Progress;

public record ExerciseProgressPoint(
    Guid SessionId,
    DateTime SessionDate,
    int BestSetReps,
    decimal BestSetWeightKg,
    decimal TotalVolumeKg,
    int TotalSets,
    int TotalReps,
    bool IsWeightPR,
    bool IsRepPR);
