using EkoaGym.Domain.Enums;

namespace EkoaGym.Domain.Entities;

public class LeagueMembership
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public LeagueRoom Room { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public decimal Points { get; set; }
    public LeagueMovement MovementSincePreviousWeek { get; set; }
    public DateTime JoinedAt { get; set; }
}
