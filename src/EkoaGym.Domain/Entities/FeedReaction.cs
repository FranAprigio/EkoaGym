namespace EkoaGym.Domain.Entities;

// "Bater Pesos" reaction (M-10.6) — one row per (Session, User), toggled on/off (never incremented).
public class FeedReaction
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
