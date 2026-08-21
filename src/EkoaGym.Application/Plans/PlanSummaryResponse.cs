namespace EkoaGym.Application.Plans;

public record PlanSummaryResponse(Guid Id, string Name, int ExerciseCount, DateTime CreatedAt, DateTime UpdatedAt);
