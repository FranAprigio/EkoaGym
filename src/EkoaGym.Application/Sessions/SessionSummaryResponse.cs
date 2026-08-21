namespace EkoaGym.Application.Sessions;

public record SessionSummaryResponse(
    Guid Id,
    Guid PlanId,
    string PlanName,
    DateTime StartedAt,
    DateTime? EndedAt,
    TimeSpan? Duration,
    int ExerciseCount,
    decimal TotalVolumeKg);
