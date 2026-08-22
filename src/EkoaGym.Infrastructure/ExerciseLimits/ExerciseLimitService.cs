using EkoaGym.Application.ExerciseLimits;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.ExerciseLimits;

public class ExerciseLimitService(EkoaGymDbContext dbContext) : IExerciseLimitService
{
    public async Task<IReadOnlyList<ExerciseLimitResponse>> GetLimitsAsync(CancellationToken cancellationToken = default)
    {
        var exercises = await dbContext.Exercises
            .Select(e => new { e.Id, e.Name, e.MuscleGroup })
            .ToListAsync(cancellationToken);

        var explicitLimits = await dbContext.ExerciseLimits
            .ToDictionaryAsync(l => l.ExerciseId, l => l.MaxWeightKg, cancellationToken);

        return exercises
            .Select(e => new ExerciseLimitResponse(
                e.Id,
                e.Name,
                explicitLimits.TryGetValue(e.Id, out var explicitCap) ? explicitCap : ExerciseLimitDefaults.GetDefaultCap(e.MuscleGroup)))
            .ToList();
    }
}
