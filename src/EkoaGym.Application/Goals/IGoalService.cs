namespace EkoaGym.Application.Goals;

public interface IGoalService
{
    Task<IReadOnlyList<GoalResponse>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GoalResponse> GetGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default);
    Task<GoalResponse> CreateGoalAsync(Guid userId, CreateGoalRequest request, CancellationToken cancellationToken = default);
    Task<GoalResponse> UpdateGoalAsync(Guid userId, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default);
    Task DeleteGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default);
}
