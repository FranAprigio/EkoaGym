using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Only the single most recently completed session is cached (per the M-0.3 acceptance criteria).
public class CachedSession
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public DateTime CompletedAt { get; set; }

    public string PayloadJson { get; set; } = string.Empty;
}
