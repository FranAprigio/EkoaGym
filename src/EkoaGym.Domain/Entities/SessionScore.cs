namespace EkoaGym.Domain.Entities;

// Computed once per session (M-10.2) and persisted so re-fetching GET .../score is idempotent and
// never double-adds points to the user's current LeagueMembership.
public class SessionScore
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public decimal ConsistencyBonus { get; set; }
    public decimal PrBonus { get; set; }

    // Per-exercise breakdown (exercise name/score) — a small JSON blob rather than a child table,
    // since it's write-once/read-only display data with no querying needs of its own.
    public string BreakdownJson { get; set; } = "[]";
    public DateTime ComputedAt { get; set; }
}
