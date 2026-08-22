namespace EkoaGym.Application.Scoring;

public record ExerciseScoreBreakdown(Guid ExerciseId, string ExerciseName, decimal Score, bool HasPendingSets);

public record SessionScoreResponse(
    Guid SessionId,
    decimal TotalScore,
    decimal ConsistencyBonus,
    decimal PrBonus,
    IReadOnlyList<ExerciseScoreBreakdown> ExerciseBreakdown);
