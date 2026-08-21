namespace EkoaGym.Application.Sessions;

public record SessionDetailResponse(
    Guid Id,
    Guid PlanId,
    string PlanName,
    DateTime StartedAt,
    DateTime? EndedAt,
    IReadOnlyList<SessionSetLogResponse> Sets);
