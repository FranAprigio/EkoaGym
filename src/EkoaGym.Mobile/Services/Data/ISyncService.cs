using EkoaGym.Mobile.Models;
using EkoaGym.Mobile.Services.Data.Entities;

namespace EkoaGym.Mobile.Services.Data;

public interface ISyncService
{
    // Refreshes the local cache from the API. Safe to call when offline — falls back silently.
    Task RefreshAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PlanSummary>> GetPlansAsync(CancellationToken ct = default);

    Task<IReadOnlyList<ExerciseSummary>> GetExercisesAsync(CancellationToken ct = default);

    // Server-side search/filter/pagination when online; falls back to filtering the cached library when offline.
    Task<PagedResult<ExerciseSummary>> SearchExercisesAsync(ExerciseSearchFilter filter, CancellationToken ct = default);

    Task<SessionSummary?> GetLastSessionAsync(CancellationToken ct = default);

    // Queues a write (e.g. logging a set) made while offline, to be replayed once connectivity returns.
    Task QueueWriteAsync(HttpMethod method, string requestUri, object? body, CancellationToken ct = default);

    Task RetryQueuedWritesAsync(CancellationToken ct = default);
}
