namespace EkoaGym.Domain.Entities;

using EkoaGym.Domain.Enums;

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

    // M-10.4: >30% above the user's 90-day PR for this exercise — league points for this set are
    // suspended (see ScoringService) until a VerificationRequest resolves it.
    public bool IsSuspicious { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.None;
}
