using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Plans;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.Plans;

public class ProgressionService(EkoaGymDbContext dbContext, IOptions<ProgressionOptions> progressionOptions) : IProgressionService
{
    private readonly ProgressionOptions _options = progressionOptions.Value;

    public async Task<NextTargetResponse> GetNextTargetAsync(Guid userId, Guid planId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.WorkoutPlans.SingleOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, cancellationToken);
        if (plan is null)
        {
            throw new PlanNotFoundException();
        }

        if (plan.UserId != userId)
        {
            throw new PlanAccessDeniedException();
        }

        var planExercise = await dbContext.WorkoutPlanExercises
            .SingleOrDefaultAsync(pe => pe.PlanId == planId && pe.ExerciseId == exerciseId, cancellationToken);
        if (planExercise is null)
        {
            throw new PlanExerciseNotFoundException();
        }

        // Most recent completed session for this plan/exercise combo, if any.
        var latestSessionId = await dbContext.SessionSetLogs
            .Where(s => s.UserId == userId && s.PlanId == planId && s.ExerciseId == exerciseId)
            .OrderByDescending(s => s.CompletedAt)
            .Select(s => (Guid?)s.SessionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSessionId is null)
        {
            // No history yet: suggest the plan's original planned values.
            return new NextTargetResponse(planExercise.Sets, planExercise.Reps, planExercise.WeightKg);
        }

        var lastSessionSets = await dbContext.SessionSetLogs
            .Where(s => s.SessionId == latestSessionId.Value)
            .ToListAsync(cancellationToken);

        var metOrExceededTarget = lastSessionSets.Count > 0
            && lastSessionSets.All(s => s.Reps >= planExercise.Reps && s.WeightKg >= planExercise.WeightKg);

        var suggestedWeightKg = metOrExceededTarget
            ? planExercise.WeightKg + _options.WeightIncrementKg
            : planExercise.WeightKg;

        return new NextTargetResponse(planExercise.Sets, planExercise.Reps, suggestedWeightKg);
    }
}
