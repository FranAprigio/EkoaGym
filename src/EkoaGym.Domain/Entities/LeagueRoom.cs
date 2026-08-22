using EkoaGym.Domain.Enums;

namespace EkoaGym.Domain.Entities;

// Up to 30 users per room/tier/week (M-10.1). A new room is opened once the current one for that
// tier+week fills up.
public class LeagueRoom
{
    public Guid Id { get; set; }
    public LeagueTier Tier { get; set; }
    public DateOnly WeekStartDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
