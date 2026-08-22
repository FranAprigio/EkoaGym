using SQLite;

namespace EkoaGym.Mobile.Services.Data.Entities;

// Single-row cache of the most recent GET api/feed (page 1) response — same single-row blob pattern
// as CachedActiveSession/CachedSession, since M-10.6 only needs "the last-cached page" when offline.
public class CachedFeed
{
    [PrimaryKey]
    public string Id { get; set; } = "current";

    public string PayloadJson { get; set; } = string.Empty;
}
