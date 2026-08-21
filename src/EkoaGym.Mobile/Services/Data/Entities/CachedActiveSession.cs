using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Single-row table holding the current in-progress workout, if any (see ActiveWorkoutState).
public class CachedActiveSession
{
    [PrimaryKey]
    public string Id { get; set; } = "current";

    public string PayloadJson { get; set; } = string.Empty;
}
