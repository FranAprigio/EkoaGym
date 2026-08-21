namespace EkoaGym.Mobile.Models;

public record GoalSummary(Guid Id, string Type, decimal TargetValue, DateOnly TargetDate, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateGoalRequest(string Type, decimal TargetValue, DateOnly TargetDate);

public record UpdateGoalRequest(string Type, decimal TargetValue, DateOnly TargetDate);
