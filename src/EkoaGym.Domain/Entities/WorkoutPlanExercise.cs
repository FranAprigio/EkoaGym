namespace EkoaGym.Domain.Entities;

public class WorkoutPlanExercise
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public WorkoutPlan Plan { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int RestSeconds { get; set; }
    public int OrderIndex { get; set; }
}
