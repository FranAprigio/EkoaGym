using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Single-row table holding the cached api/exercises/limits payload (M-10.3) — same single-row
// pattern as CachedActiveSession, since it's one whole-list blob rather than per-row data.
public class CachedExerciseLimits
{
    [PrimaryKey]
    public string Id { get; set; } = "current";

    public string PayloadJson { get; set; } = string.Empty;
}
