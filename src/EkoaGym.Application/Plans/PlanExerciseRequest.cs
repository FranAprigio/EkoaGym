using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Plans;

public record PlanExerciseRequest(
    [Required] Guid ExerciseId,
    [Range(1, 20)] int Sets,
    [Range(1, 100)] int Reps,
    [Range(0, 500)] decimal WeightKg,
    [Range(0, 600)] int RestSeconds);
