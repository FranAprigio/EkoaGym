namespace EkoaGym.Domain.Entities;

// Server-tunable per-exercise weight cap (M-10.3) — the app fetches these rather than hardcoding them.
public class ExerciseLimit
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public decimal MaxWeightKg { get; set; }
}
