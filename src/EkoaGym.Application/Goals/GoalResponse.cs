namespace EkoaGym.Application.Goals;

public record GoalResponse(
    Guid Id,
    string Type,
    decimal TargetValue,
    DateOnly TargetDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);
