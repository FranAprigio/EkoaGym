namespace EkoaGym.Application.Plans;

public record PlanDetailResponse(
    Guid Id,
    string Name,
    IReadOnlyList<PlanExerciseResponse> Exercises,
    DateTime CreatedAt,
    DateTime UpdatedAt);
