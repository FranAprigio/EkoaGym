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
}

public class LoggedSet
{
    public int SetNumber { get; set; }
    public int ActualReps { get; set; }
    public decimal ActualWeightKg { get; set; }
    public DateTime CompletedAt { get; set; }

    // False while this set's POST api/sessions/{id}/sets call is still queued offline.
    public bool Synced { get; set; }
}
