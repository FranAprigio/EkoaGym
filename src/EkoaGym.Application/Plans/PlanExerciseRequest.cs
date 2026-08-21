using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Plans;

public record PlanExerciseRequest(
    [property: Required] Guid ExerciseId,
    [property: Range(1, 20)] int Sets,
    [property: Range(1, 100)] int Reps,
    [property: Range(0, 500)] decimal WeightKg,
    [property: Range(0, 600)] int RestSeconds);
