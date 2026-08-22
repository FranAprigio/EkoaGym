namespace EkoaGym.Mobile.Models;

public record ExerciseScoreBreakdown(Guid ExerciseId, string ExerciseName, decimal Score, bool HasPendingSets);

public record SessionScore(
    Guid SessionId,
    decimal TotalScore,
    decimal ConsistencyBonus,
    decimal PrBonus,
    IReadOnlyList<ExerciseScoreBreakdown> ExerciseBreakdown);
