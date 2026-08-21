using System.Text.Json;
using EkoaGym.Mobile.Models;
using EkoaGym.Mobile.Services.Api;
using EkoaGym.Mobile.Services.Connectivity;
using EkoaGym.Mobile.Services.Data.Entities;

namespace EkoaGym.Mobile.Services.Data;

public class SyncService : ISyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ApiClient apiClient;
    private readonly LocalDatabaseService db;

    public SyncService(ApiClient apiClient, LocalDatabaseService db, IConnectivityService connectivity)
    {
        this.apiClient = apiClient;
        this.db = db;

        // Retry any queued offline writes automatically as soon as the network comes back.
        connectivity.ConnectivityChanged += async (_, isConnected) =>
        {
            if (isConnected)
            {
                await RetryQueuedWritesAsync();
            }
        };
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var plans = await apiClient.GetAsync<List<PlanSummary>>("api/plans", ct) ?? [];
            await db.ReplacePlansAsync(plans.Select(ToEntity));
        }
        catch (ApiException)
        {
            // Keep whatever is already cached — the caller can still read GetPlansAsync().
        }

        try
        {
            var exercises = await apiClient.GetAsync<PagedResult<ExerciseSummary>>("api/exercises?page=1&pageSize=100", ct);
            if (exercises is not null)
            {
                await db.ReplaceExercisesAsync(exercises.Items.Select(ToEntity));
            }
        }
        catch (ApiException)
        {
            // Same fallback as above — the exercise library rarely changes.
        }
    }

    public async Task<IReadOnlyList<PlanSummary>> GetPlansAsync(CancellationToken ct = default)
    {
        try
        {
            var plans = await apiClient.GetAsync<List<PlanSummary>>("api/plans", ct) ?? [];
            await db.ReplacePlansAsync(plans.Select(ToEntity));
            return plans;
        }
        catch (ApiException)
        {
            var cached = await db.GetPlansAsync();
            return cached.Select(c => JsonSerializer.Deserialize<PlanSummary>(c.PayloadJson, JsonOptions)!).ToList();
        }
    }

    public async Task<IReadOnlyList<ExerciseSummary>> GetExercisesAsync(CancellationToken ct = default)
    {
        try
        {
            var exercises = await apiClient.GetAsync<PagedResult<ExerciseSummary>>("api/exercises?page=1&pageSize=100", ct);
            var items = exercises?.Items ?? [];
            await db.ReplaceExercisesAsync(items.Select(ToEntity));
            return items;
        }
        catch (ApiException)
        {
            var cached = await db.GetExercisesAsync();
            return cached.Select(c => JsonSerializer.Deserialize<ExerciseSummary>(c.PayloadJson, JsonOptions)!).ToList();
        }
    }

    public async Task<SessionSummary?> GetLastSessionAsync(CancellationToken ct = default)
    {
        try
        {
            var sessions = await apiClient.GetAsync<PagedResult<SessionSummary>>("api/sessions?page=1&pageSize=1", ct);
            var latest = sessions?.Items.FirstOrDefault();
            if (latest is not null)
            {
                await db.SaveLastSessionAsync(ToEntity(latest));
            }

            return latest;
        }
        catch (ApiException)
        {
            var cached = await db.GetLastSessionAsync();
            return cached is null ? null : JsonSerializer.Deserialize<SessionSummary>(cached.PayloadJson, JsonOptions);
        }
    }

    public Task QueueWriteAsync(HttpMethod method, string requestUri, object? body, CancellationToken ct = default) =>
        db.EnqueueWriteAsync(new OfflineWriteQueueItem
        {
            Method = method.Method,
            RequestUri = requestUri,
            PayloadJson = body is null ? null : JsonSerializer.Serialize(body, JsonOptions),
            CreatedAt = DateTime.UtcNow,
        });

    public async Task RetryQueuedWritesAsync(CancellationToken ct = default)
    {
        var queued = await db.GetQueuedWritesAsync();
        foreach (var item in queued)
        {
            try
            {
                object? body = item.PayloadJson is null ? null : JsonSerializer.Deserialize<JsonElement>(item.PayloadJson, JsonOptions);
                switch (item.Method.ToUpperInvariant())
                {
                    case "POST":
                        await apiClient.PostAsync(item.RequestUri, body, ct);
                        break;
                    case "PUT":
                        await apiClient.PutAsync(item.RequestUri, body, ct);
                        break;
                    case "DELETE":
                        await apiClient.DeleteAsync(item.RequestUri, ct);
                        break;
                }

                await db.RemoveQueuedWriteAsync(item.Id);
            }
            catch (ApiException)
            {
                item.RetryCount++;
                await db.UpdateQueuedWriteAsync(item);
            }
        }
    }

    private static CachedWorkoutPlan ToEntity(PlanSummary plan) => new()
    {
        Id = plan.Id.ToString(),
        Name = plan.Name,
        PayloadJson = JsonSerializer.Serialize(plan, JsonOptions),
        CachedAt = DateTime.UtcNow,
    };

    private static CachedExercise ToEntity(ExerciseSummary exercise) => new()
    {
        Id = exercise.Id.ToString(),
        Name = exercise.Name,
        MuscleGroup = exercise.MuscleGroup,
        PayloadJson = JsonSerializer.Serialize(exercise, JsonOptions),
        CachedAt = DateTime.UtcNow,
    };

    private static CachedSession ToEntity(SessionSummary session) => new()
    {
        Id = session.Id.ToString(),
        CompletedAt = session.EndedAt ?? session.StartedAt,
        PayloadJson = JsonSerializer.Serialize(session, JsonOptions),
    };
}
