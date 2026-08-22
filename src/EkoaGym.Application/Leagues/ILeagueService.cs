namespace EkoaGym.Application.Leagues;

public interface ILeagueService
{
    // tzOffsetMinutes: the caller's local UTC offset (e.g. from DateTimeOffset.Now.Offset), used to
    // compute the Monday 00:00 -> Sunday 23:59 week boundary in the user's own local time (M-10.1).
    Task<LeagueCurrentResponse> GetCurrentAsync(Guid userId, int tzOffsetMinutes, CancellationToken cancellationToken = default);

    // Credits a completed session's score (M-10.2) to the user's current-week league membership,
    // lazily creating that membership (with correct tier carry-over) if this is their first activity this week.
    Task AddPointsAsync(Guid userId, decimal points, CancellationToken cancellationToken = default);
}
