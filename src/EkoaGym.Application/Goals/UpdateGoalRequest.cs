using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Goals;

public record UpdateGoalRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Type,
    [Range(0.01, double.MaxValue)] decimal TargetValue,
    [Required, NotInPast] DateOnly TargetDate);
