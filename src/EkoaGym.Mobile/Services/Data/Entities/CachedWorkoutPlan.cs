using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Cached copy of a WorkoutPlan summary. PayloadJson keeps the full API response so screens can
// deserialize whatever shape they need without a second relational schema to maintain offline.
public class CachedWorkoutPlan
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; }
}
