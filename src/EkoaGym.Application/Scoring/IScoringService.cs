namespace EkoaGym.Application.Scoring;

public interface IScoringService
{
    // Idempotent: computed once and persisted (SessionScore) on first call, returned as-is afterwards
    // so league points are never double-added on a repeated fetch.
    Task<SessionScoreResponse> GetOrComputeScoreAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
}
