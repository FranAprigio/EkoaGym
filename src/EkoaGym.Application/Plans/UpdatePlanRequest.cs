using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Plans;

public record UpdatePlanRequest(
    [property: Required, StringLength(200, MinimumLength = 1)] string Name,
    [property: Required, MinLength(1)] IReadOnlyList<PlanExerciseRequest> Exercises);
