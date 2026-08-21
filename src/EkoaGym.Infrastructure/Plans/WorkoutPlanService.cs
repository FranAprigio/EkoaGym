using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Plans;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Plans;

public class WorkoutPlanService(EkoaGymDbContext dbContext) : IWorkoutPlanService
{
    public async Task<IReadOnlyList<PlanSummaryResponse>> GetPlansAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkoutPlans
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlanSummaryResponse(
                p.Id,
                p.Name,
                dbContext.WorkoutPlanExercises.Count(pe => pe.PlanId == p.Id),
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanDetailResponse> GetPlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await FindOwnedPlanAsync(userId, planId, cancellationToken);
        return await ToDetailAsync(plan, cancellationToken);
    }

    public async Task<PlanDetailResponse> CreatePlanAsync(Guid userId, CreatePlanRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureExercisesExistAsync(request.Exercises.Select(e => e.ExerciseId), cancellationToken);

        var plan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.WorkoutPlans.Add(plan);
        dbContext.WorkoutPlanExercises.AddRange(BuildPlanExercises(plan.Id, request.Exercises));

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ToDetailAsync(plan, cancellationToken);
    }

    public async Task<PlanDetailResponse> UpdatePlanAsync(Guid userId, Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await FindOwnedPlanAsync(userId, planId, cancellationToken);
        await EnsureExercisesExistAsync(request.Exercises.Select(e => e.ExerciseId), cancellationToken);

        var existingExercises = await dbContext.WorkoutPlanExercises.Where(pe => pe.PlanId == plan.Id).ToListAsync(cancellationToken);
        dbContext.WorkoutPlanExercises.RemoveRange(existingExercises);
        dbContext.WorkoutPlanExercises.AddRange(BuildPlanExercises(plan.Id, request.Exercises));

        plan.Name = request.Name.Trim();
        plan.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ToDetailAsync(plan, cancellationToken);
    }

    public async Task DeletePlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await FindOwnedPlanAsync(userId, planId, cancellationToken);

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlanDetailResponse> DuplicatePlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default)
    {
        var plan = await FindOwnedPlanAsync(userId, planId, cancellationToken);
        var sourceExercises = await dbContext.WorkoutPlanExercises
            .Where(pe => pe.PlanId == plan.Id)
            .OrderBy(pe => pe.OrderIndex)
            .ToListAsync(cancellationToken);

        var copy = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = $"{plan.Name} (copy)",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.WorkoutPlans.Add(copy);
        dbContext.WorkoutPlanExercises.AddRange(sourceExercises.Select(pe => new WorkoutPlanExercise
        {
            Id = Guid.NewGuid(),
            PlanId = copy.Id,
            ExerciseId = pe.ExerciseId,
            Sets = pe.Sets,
            Reps = pe.Reps,
            WeightKg = pe.WeightKg,
            RestSeconds = pe.RestSeconds,
            OrderIndex = pe.OrderIndex
        }));

        await dbContext.SaveChangesAsync(cancellationToken);

        return await ToDetailAsync(copy, cancellationToken);
    }

    private async Task<WorkoutPlan> FindOwnedPlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken)
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

        return plan;
    }

    private async Task EnsureExercisesExistAsync(IEnumerable<Guid> exerciseIds, CancellationToken cancellationToken)
    {
        var distinctIds = exerciseIds.Distinct().ToList();
        var existingCount = await dbContext.Exercises.CountAsync(e => distinctIds.Contains(e.Id), cancellationToken);
        if (existingCount != distinctIds.Count)
        {
            throw new ExerciseNotFoundException();
        }
    }

    private static IEnumerable<WorkoutPlanExercise> BuildPlanExercises(Guid planId, IReadOnlyList<PlanExerciseRequest> exercises) =>
        exercises.Select((e, index) => new WorkoutPlanExercise
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            ExerciseId = e.ExerciseId,
            Sets = e.Sets,
            Reps = e.Reps,
            WeightKg = e.WeightKg,
            RestSeconds = e.RestSeconds,
            OrderIndex = index
        });

    private async Task<PlanDetailResponse> ToDetailAsync(WorkoutPlan plan, CancellationToken cancellationToken)
    {
        var exercises = await dbContext.WorkoutPlanExercises
            .Where(pe => pe.PlanId == plan.Id)
            .OrderBy(pe => pe.OrderIndex)
            .Join(dbContext.Exercises, pe => pe.ExerciseId, e => e.Id, (pe, e) => new PlanExerciseResponse(
                pe.Id, pe.ExerciseId, e.Name, pe.Sets, pe.Reps, pe.WeightKg, pe.RestSeconds, pe.OrderIndex))
            .ToListAsync(cancellationToken);

        return new PlanDetailResponse(plan.Id, plan.Name, exercises, plan.CreatedAt, plan.UpdatedAt);
    }
}
