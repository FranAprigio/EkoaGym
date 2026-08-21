using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Goals;

// Type is validated for presence/length only; known suggestions live in Domain.Constants.GoalTypes,
// but any other non-empty value is accepted as a free-text custom goal.
public record CreateGoalRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Type,
    [property: Range(0.01, double.MaxValue)] decimal TargetValue,
    [property: Required, NotInPast] DateOnly TargetDate);
