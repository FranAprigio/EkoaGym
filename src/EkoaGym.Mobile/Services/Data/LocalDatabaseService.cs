using EkoaGym.Mobile.Services.Data.Entities;
using Microsoft.Maui.Storage;
using SQLite;

namespace EkoaGym.Mobile.Services.Data;

// Local SQLite cache used for offline access to plans/exercises/last session, plus a queue for
// writes made while offline. One connection per app instance, opened lazily on first use.
public class LocalDatabaseService
{
    private readonly SemaphoreSlim initLock = new(1, 1);
    private SQLiteAsyncConnection? connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (connection is not null)
        {
            return connection;
        }

        await initLock.WaitAsync();
        try
        {
            if (connection is null)
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ekoagym_cache.db3");
                var conn = new SQLiteAsyncConnection(dbPath);
                await conn.CreateTableAsync<CachedWorkoutPlan>();
                await conn.CreateTableAsync<CachedExercise>();
                await conn.CreateTableAsync<CachedSession>();
                await conn.CreateTableAsync<OfflineWriteQueueItem>();
                connection = conn;
            }
        }
        finally
        {
            initLock.Release();
        }

        return connection;
    }

    public async Task<List<CachedWorkoutPlan>> GetPlansAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<CachedWorkoutPlan>().ToListAsync();
    }

    public async Task ReplacePlansAsync(IEnumerable<CachedWorkoutPlan> plans)
    {
        var conn = await GetConnectionAsync();
        await conn.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<CachedWorkoutPlan>();
            tran.InsertAll(plans);
        });
    }

    public async Task<List<CachedExercise>> GetExercisesAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<CachedExercise>().ToListAsync();
    }

    public async Task ReplaceExercisesAsync(IEnumerable<CachedExercise> exercises)
    {
        var conn = await GetConnectionAsync();
        await conn.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<CachedExercise>();
            tran.InsertAll(exercises);
        });
    }

    // Used for partial/paged search results — inserts/updates without evicting the rest of the cached library.
    public async Task UpsertExercisesAsync(IEnumerable<CachedExercise> exercises)
    {
        var conn = await GetConnectionAsync();
        await conn.RunInTransactionAsync(tran =>
        {
            foreach (var exercise in exercises)
            {
                tran.InsertOrReplace(exercise);
            }
        });
    }

    public async Task<CachedSession?> GetLastSessionAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<CachedSession>().OrderByDescending(s => s.CompletedAt).FirstOrDefaultAsync();
    }

    public async Task SaveLastSessionAsync(CachedSession session)
    {
        var conn = await GetConnectionAsync();
        await conn.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<CachedSession>();
            tran.Insert(session);
        });
    }

    public async Task<List<OfflineWriteQueueItem>> GetQueuedWritesAsync()
    {
        var conn = await GetConnectionAsync();
        return await conn.Table<OfflineWriteQueueItem>().OrderBy(i => i.CreatedAt).ToListAsync();
    }

    public async Task EnqueueWriteAsync(OfflineWriteQueueItem item)
    {
        var conn = await GetConnectionAsync();
        await conn.InsertAsync(item);
    }

    public async Task RemoveQueuedWriteAsync(int id)
    {
        var conn = await GetConnectionAsync();
        await conn.DeleteAsync<OfflineWriteQueueItem>(id);
    }

    public async Task UpdateQueuedWriteAsync(OfflineWriteQueueItem item)
    {
        var conn = await GetConnectionAsync();
        await conn.UpdateAsync(item);
    }
}
