using EkoaGym.Application.Plans;

namespace EkoaGym.Application.Sessions;

public record StartSessionResponse(Guid SessionId, Guid PlanId, DateTime StartedAt, IReadOnlyList<PlanExerciseResponse> Exercises);
