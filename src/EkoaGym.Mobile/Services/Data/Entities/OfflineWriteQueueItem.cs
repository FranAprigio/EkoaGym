using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// A write the user made while offline, replayed against the API once connectivity returns.
public class OfflineWriteQueueItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Method { get; set; } = string.Empty;

    public string RequestUri { get; set; } = string.Empty;

    public string? PayloadJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public int RetryCount { get; set; }
}
