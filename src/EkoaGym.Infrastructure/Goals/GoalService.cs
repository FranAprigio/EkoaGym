using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Goals;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Goals;

public class GoalService(EkoaGymDbContext dbContext) : IGoalService
{
    public async Task<IReadOnlyList<GoalResponse>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Goals
            .Where(g => g.UserId == userId && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => ToResponse(g))
            .ToListAsync(cancellationToken);
    }

    public async Task<GoalResponse> GetGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default)
    {
        var goal = await FindOwnedGoalAsync(userId, goalId, cancellationToken);
        return ToResponse(goal);
    }

    public async Task<GoalResponse> CreateGoalAsync(Guid userId, CreateGoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = request.Type.Trim(),
            TargetValue = request.TargetValue,
            TargetDate = request.TargetDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Goals.Add(goal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(goal);
    }

    public async Task<GoalResponse> UpdateGoalAsync(Guid userId, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = await FindOwnedGoalAsync(userId, goalId, cancellationToken);

        goal.Type = request.Type.Trim();
        goal.TargetValue = request.TargetValue;
        goal.TargetDate = request.TargetDate;
        goal.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(goal);
    }

    public async Task DeleteGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken = default)
    {
        var goal = await FindOwnedGoalAsync(userId, goalId, cancellationToken);

        goal.IsDeleted = true;
        goal.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Goal> FindOwnedGoalAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        // Not found (rather than forbidden) for goals owned by another user, to avoid leaking existence.
        var goal = await dbContext.Goals
            .SingleOrDefaultAsync(g => g.Id == goalId && g.UserId == userId && !g.IsDeleted, cancellationToken);

        return goal ?? throw new GoalNotFoundException();
    }

    private static GoalResponse ToResponse(Goal goal) => new(
        goal.Id,
        goal.Type,
        goal.TargetValue,
        goal.TargetDate,
        goal.CreatedAt,
        goal.UpdatedAt);
}
