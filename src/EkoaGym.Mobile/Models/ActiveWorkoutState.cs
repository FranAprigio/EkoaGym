namespace EkoaGym.Mobile.Models;

// Locally persisted snapshot of the in-progress workout — survives app close/pause and lets
// ActiveWorkout.razor resume exactly where the user left off without losing logged sets.
public class ActiveWorkoutState
{
    public Guid SessionId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<ActiveWorkoutExercise> Exercises { get; set; } = [];
    public int CurrentExerciseIndex { get; set; }
    public DateTime? RestEndsAtUtc { get; set; }
}

public class ActiveWorkoutExercise
{
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int RestSeconds { get; set; }
    public List<LoggedSet> LoggedSets { get; set; } = [];

    // M-9.3 mid-session PR detection baseline — the best weight/reps achieved BEFORE this session
    // (from GET api/exercises/{id}/progress, fetched once lazily), updated as this session improves on it.
    public decimal? BestWeightKgBeforeSession { get; set; }
    public int? BestRepsAtBestWeightBeforeSession { get; set; }
    public bool ProgressBaselineLoaded { get; set; }
}

public class LoggedSet
{
    // Server-generated SessionSetLog id — null while a set is still queued offline (see M-10.4:
    // verification submission needs this once the set has actually synced).
    public Guid? Id { get; set; }
    public int SetNumber { get; set; }
    public int ActualReps { get; set; }
    public decimal ActualWeightKg { get; set; }
    public DateTime CompletedAt { get; set; }

    // False while this set's POST api/sessions/{id}/sets call is still queued offline.
    public bool Synced { get; set; }

    // M-9.3: mirrors the server's isWeightPR/isRepPR semantics (M-7.3) client-side, computed against
    // ActiveWorkoutExercise's "before session" baseline at the moment this set was logged.
    public bool IsWeightPR { get; set; }
    public bool IsRepPR { get; set; }

    // M-10.4: true when this set is >30% above the user's 90-day PR — league points are suspended
    // and the user needs to submit photo/video proof.
    public bool IsSuspicious { get; set; }
    public string VerificationStatus { get; set; } = "None";

    // Client-only — hides the "submit proof" button once the user has uploaded something, even
    // though the server-side status stays PendingReview until a moderator resolves it.
    public bool ProofSubmitted { get; set; }
}
