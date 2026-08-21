using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence.Seed;
using EkoaGym.Infrastructure.Plans;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.Tests.Plans;

public class ProgressionServiceTests
{
    private static readonly Guid SeededExerciseId = ExerciseSeedData.All[0].Id;

    private static ProgressionService CreateSut(out Persistence.EkoaGymDbContext dbContext, decimal weightIncrementKg = 2.5m)
    {
        dbContext = TestDbContextFactory.Create();
        var options = Options.Create(new ProgressionOptions { WeightIncrementKg = weightIncrementKg });
        return new ProgressionService(dbContext, options);
    }

    private static (WorkoutPlan Plan, WorkoutPlanExercise PlanExercise) SeedPlan(Persistence.EkoaGymDbContext dbContext, Guid userId)
    {
        var plan = new WorkoutPlan { Id = Guid.NewGuid(), UserId = userId, Name = "Plan", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var planExercise = new WorkoutPlanExercise
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            ExerciseId = SeededExerciseId,
            Sets = 3,
            Reps = 10,
            WeightKg = 20,
            RestSeconds = 90,
            OrderIndex = 0
        };
        dbContext.WorkoutPlans.Add(plan);
        dbContext.WorkoutPlanExercises.Add(planExercise);
        dbContext.SaveChanges();
        return (plan, planExercise);
    }

    private static void AddCompletedSessionWithSets(
        Persistence.EkoaGymDbContext dbContext, Guid userId, Guid planId, DateTime startedAt, params (int Reps, decimal WeightKg)[] sets)
    {
        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            StartedAt = startedAt,
            EndedAt = startedAt.AddMinutes(45),
            CreatedAt = startedAt,
            UpdatedAt = startedAt
        };
        dbContext.WorkoutSessions.Add(session);

        for (var i = 0; i < sets.Length; i++)
        {
            dbContext.SessionSetLogs.Add(new SessionSetLog
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                ExerciseId = SeededExerciseId,
                SetNumber = i + 1,
                ActualReps = sets[i].Reps,
                ActualWeightKg = sets[i].WeightKg,
                CompletedAt = startedAt.AddMinutes(i)
            });
        }

        dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetNextTargetAsync_NoHistory_ReturnsPlannedValues()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        var (plan, planExercise) = SeedPlan(dbContext, userId);

        var result = await sut.GetNextTargetAsync(userId, plan.Id, SeededExerciseId);

        Assert.Equal(planExercise.Sets, result.Sets);
        Assert.Equal(planExercise.Reps, result.Reps);
        Assert.Equal(planExercise.WeightKg, result.WeightKg);
    }

    [Fact]
    public async Task GetNextTargetAsync_AllSetsMetOrExceededTarget_SuggestsWeightIncrease()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        var (plan, planExercise) = SeedPlan(dbContext, userId);
        AddCompletedSessionWithSets(dbContext, userId, plan.Id, DateTime.UtcNow.AddDays(-1), (10, 20m), (10, 20m), (10, 20m));

        var result = await sut.GetNextTargetAsync(userId, plan.Id, SeededExerciseId);

        Assert.Equal(planExercise.WeightKg + 2.5m, result.WeightKg);
    }

    [Fact]
    public async Task GetNextTargetAsync_SomeSetsMissedTarget_RepeatsPlannedValues()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        var (plan, planExercise) = SeedPlan(dbContext, userId);
        AddCompletedSessionWithSets(dbContext, userId, plan.Id, DateTime.UtcNow.AddDays(-1), (10, 20m), (8, 20m), (10, 20m));

        var result = await sut.GetNextTargetAsync(userId, plan.Id, SeededExerciseId);

        Assert.Equal(planExercise.WeightKg, result.WeightKg);
    }

    [Fact]
    public async Task GetNextTargetAsync_OnlyConsidersCompletedSessions()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        var (plan, planExercise) = SeedPlan(dbContext, userId);

        // In-progress session (no EndedAt) with sets that would otherwise trigger a PR/increase.
        var inProgressSession = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            StartedAt = DateTime.UtcNow,
            EndedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.WorkoutSessions.Add(inProgressSession);
        dbContext.SessionSetLogs.Add(new SessionSetLog
        {
            Id = Guid.NewGuid(),
            SessionId = inProgressSession.Id,
            ExerciseId = SeededExerciseId,
            SetNumber = 1,
            ActualReps = 10,
            ActualWeightKg = 20,
            CompletedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();

        var result = await sut.GetNextTargetAsync(userId, plan.Id, SeededExerciseId);

        // Should behave as "no history" since the only session isn't completed yet.
        Assert.Equal(planExercise.WeightKg, result.WeightKg);
    }

    [Fact]
    public async Task GetNextTargetAsync_ExerciseNotInPlan_ThrowsPlanExerciseNotFoundException()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        var (plan, _) = SeedPlan(dbContext, userId);

        await Assert.ThrowsAsync<PlanExerciseNotFoundException>(
            () => sut.GetNextTargetAsync(userId, plan.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetNextTargetAsync_AnotherUsersPlan_ThrowsPlanAccessDeniedException()
    {
        var sut = CreateSut(out var dbContext);
        var (plan, _) = SeedPlan(dbContext, Guid.NewGuid());

        await Assert.ThrowsAsync<PlanAccessDeniedException>(
            () => sut.GetNextTargetAsync(Guid.NewGuid(), plan.Id, SeededExerciseId));
    }
}
