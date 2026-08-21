using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Read-only mirror of the exercise library reference data.
public class CachedExercise
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string MuscleGroup { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; }
}
