namespace EkoaGym.Mobile.Models;

public record SessionSummary(
    Guid Id,
    Guid PlanId,
    string PlanName,
    DateTime StartedAt,
    DateTime? EndedAt,
    TimeSpan? Duration,
    int ExerciseCount,
    decimal TotalVolumeKg);
