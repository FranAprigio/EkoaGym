namespace EkoaGym.Domain.Entities;

public class SessionSetLog
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public int ActualReps { get; set; }
    public decimal ActualWeightKg { get; set; }
    public DateTime CompletedAt { get; set; }
}
