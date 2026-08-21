using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Plans;

public record CreatePlanRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Name,
    [Required, MinLength(1)] IReadOnlyList<PlanExerciseRequest> Exercises);
