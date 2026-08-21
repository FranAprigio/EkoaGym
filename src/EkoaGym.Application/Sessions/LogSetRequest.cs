using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Sessions;

public record LogSetRequest(
    [property: Required] Guid ExerciseId,
    [property: Range(1, 100)] int SetNumber,
    [property: Range(0, 100)] int ActualReps,
    [property: Range(0, 500)] decimal ActualWeightKg);
