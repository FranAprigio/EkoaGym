using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Sessions;

public record LogSetRequest(
    [Required] Guid ExerciseId,
    [Range(1, 100)] int SetNumber,
    [Range(0, 100)] int ActualReps,
    [Range(0, 500)] decimal ActualWeightKg);
