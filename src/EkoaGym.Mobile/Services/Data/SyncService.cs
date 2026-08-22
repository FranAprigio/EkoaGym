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

    public async Task<PagedResult<ExerciseSummary>> SearchExercisesAsync(ExerciseSearchFilter filter, CancellationToken ct = default)
    {
        var queryParams = new List<string> { $"page={filter.Page}", $"pageSize={filter.PageSize}" };
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            queryParams.Add($"q={Uri.EscapeDataString(filter.Query)}");
        }

        if (!string.IsNullOrEmpty(filter.MuscleGroup))
        {
            queryParams.Add($"muscleGroup={filter.MuscleGroup}");
        }

        if (!string.IsNullOrEmpty(filter.Equipment))
        {
            queryParams.Add($"equipment={filter.Equipment}");
        }

        if (!string.IsNullOrEmpty(filter.Difficulty))
        {
            queryParams.Add($"difficulty={filter.Difficulty}");
        }

        try
        {
            var result = await apiClient.GetAsync<PagedResult<ExerciseSummary>>($"api/exercises?{string.Join('&', queryParams)}", ct);
            if (result is not null)
            {
                await db.UpsertExercisesAsync(result.Items.Select(ToEntity));
            }

            return result ?? new PagedResult<ExerciseSummary>([], filter.Page, filter.PageSize, 0);
        }
        catch (ApiException)
        {
            var cached = await db.GetExercisesAsync();
            var all = cached.Select(c => JsonSerializer.Deserialize<ExerciseSummary>(c.PayloadJson, JsonOptions)!);

            var filtered = all.Where(e =>
                    (string.IsNullOrWhiteSpace(filter.Query) || e.Name.Contains(filter.Query, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(filter.MuscleGroup) || e.MuscleGroup == filter.MuscleGroup) &&
                    (string.IsNullOrEmpty(filter.Equipment) || e.Equipment == filter.Equipment) &&
                    (string.IsNullOrEmpty(filter.Difficulty) || e.Difficulty == filter.Difficulty))
                .ToList();

            var page = filtered.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            return new PagedResult<ExerciseSummary>(page, filter.Page, filter.PageSize, filtered.Count);
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

    public async Task<IReadOnlyList<ExerciseLimitInfo>> GetExerciseLimitsAsync(CancellationToken ct = default)
    {
        try
        {
            var limits = await apiClient.GetAsync<List<ExerciseLimitInfo>>("api/exercises/limits", ct) ?? [];
            await db.SaveExerciseLimitsAsync(new Entities.CachedExerciseLimits { PayloadJson = JsonSerializer.Serialize(limits, JsonOptions) });
            return limits;
        }
        catch (ApiException)
        {
            var cached = await db.GetExerciseLimitsAsync();
            return cached is null ? [] : JsonSerializer.Deserialize<List<ExerciseLimitInfo>>(cached.PayloadJson, JsonOptions) ?? [];
        }
    }

    public async Task<PagedResult<FeedCard>> GetFeedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var result = await apiClient.GetAsync<PagedResult<FeedCard>>($"api/feed?page={page}&pageSize={pageSize}", ct)
                ?? new PagedResult<FeedCard>([], page, pageSize, 0);
            if (page == 1)
            {
                await db.SaveFeedAsync(new Entities.CachedFeed { PayloadJson = JsonSerializer.Serialize(result, JsonOptions) });
            }

            return result;
        }
        catch (ApiException)
        {
            if (page != 1)
            {
                return new PagedResult<FeedCard>([], page, pageSize, 0);
            }

            var cached = await db.GetFeedAsync();
            return cached is null
                ? new PagedResult<FeedCard>([], page, pageSize, 0)
                : JsonSerializer.Deserialize<PagedResult<FeedCard>>(cached.PayloadJson, JsonOptions)!;
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
