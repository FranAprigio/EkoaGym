namespace EkoaGym.Domain.Entities;

// Minimal prerequisite for BE-4.3's progression lookup; no write endpoint exists yet (future session-logging epic).
// SessionId correlates sets performed together in one workout instance (no dedicated Session entity yet).
public class SessionSetLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public Guid ExerciseId { get; set; }
    public Guid SessionId { get; set; }
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public DateTime CompletedAt { get; set; }
}
