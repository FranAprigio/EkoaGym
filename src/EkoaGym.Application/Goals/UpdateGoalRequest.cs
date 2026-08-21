using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Goals;

public record UpdateGoalRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Type,
    [property: Range(0.01, double.MaxValue)] decimal TargetValue,
    [property: Required, NotInPast] DateOnly TargetDate);
