using EkoaGym.Application.Common;

namespace EkoaGym.Application.Sessions;

public interface IWorkoutSessionService
{
    Task<StartSessionResponse> StartSessionAsync(Guid userId, StartSessionRequest request, CancellationToken cancellationToken = default);
    Task<LogSetResponse> LogSetAsync(Guid userId, Guid sessionId, LogSetRequest request, CancellationToken cancellationToken = default);
    Task<CompleteSessionResponse> CompleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<PagedResult<SessionSummaryResponse>> GetSessionsAsync(Guid userId, SessionFilter filter, CancellationToken cancellationToken = default);
    Task<SessionDetailResponse> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
}
