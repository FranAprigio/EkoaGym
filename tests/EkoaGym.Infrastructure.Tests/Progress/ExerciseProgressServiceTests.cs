using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence.Seed;
using EkoaGym.Infrastructure.Progress;

namespace EkoaGym.Infrastructure.Tests.Progress;

public class ExerciseProgressServiceTests
{
    private static readonly Guid SeededExerciseId = ExerciseSeedData.All[0].Id;

    private static ExerciseProgressService CreateSut(out Persistence.EkoaGymDbContext dbContext)
    {
        dbContext = TestDbContextFactory.Create();
        return new ExerciseProgressService(dbContext);
    }

    private static Guid AddCompletedSession(
        Persistence.EkoaGymDbContext dbContext, Guid userId, DateTime startedAt, params (int Reps, decimal WeightKg)[] sets)
    {
        var plan = new WorkoutPlan { Id = Guid.NewGuid(), UserId = userId, Name = "Plan", CreatedAt = startedAt, UpdatedAt = startedAt };
        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            StartedAt = startedAt,
            EndedAt = startedAt.AddMinutes(45),
            CreatedAt = startedAt,
            UpdatedAt = startedAt
        };
        dbContext.WorkoutPlans.Add(plan);
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
        return session.Id;
    }

    [Fact]
    public async Task GetExerciseProgressAsync_UnknownExercise_ThrowsExerciseNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ExerciseNotFoundException>(
            () => sut.GetExerciseProgressAsync(Guid.NewGuid(), Guid.NewGuid(), null, null));
    }

    [Fact]
    public async Task GetExerciseProgressAsync_NeverLogged_ReturnsEmptyList()
    {
        var sut = CreateSut(out _);

        var result = await sut.GetExerciseProgressAsync(Guid.NewGuid(), SeededExerciseId, null, null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExerciseProgressAsync_FirstSession_FlagsBothPersonalRecords()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-2), (10, 20m), (10, 20m));

        var result = await sut.GetExerciseProgressAsync(userId, SeededExerciseId, null, null);

        var point = Assert.Single(result);
        Assert.True(point.IsWeightPR);
        Assert.True(point.IsRepPR);
        Assert.Equal(20m, point.BestSetWeightKg);
        Assert.Equal(400m, point.TotalVolumeKg); // 2 sets * 10 reps * 20kg
        Assert.Equal(2, point.TotalSets);
        Assert.Equal(20, point.TotalReps);
    }

    [Fact]
    public async Task GetExerciseProgressAsync_LaterSessionWithLowerWeight_DoesNotFlagWeightPR()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-2), (10, 30m));
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-1), (10, 25m));

        var result = await sut.GetExerciseProgressAsync(userId, SeededExerciseId, null, null);

        Assert.Equal(2, result.Count);
        Assert.False(result[1].IsWeightPR);
    }

    [Fact]
    public async Task GetExerciseProgressAsync_LaterSessionWithHigherWeight_FlagsWeightPR()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-2), (10, 20m));
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-1), (10, 25m));

        var result = await sut.GetExerciseProgressAsync(userId, SeededExerciseId, null, null);

        Assert.True(result[1].IsWeightPR);
    }

    [Fact]
    public async Task GetExerciseProgressAsync_DateRangeFilter_ExcludesOutOfRangeSessions()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddMonths(-6), (10, 20m));
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-1), (10, 22m));

        var result = await sut.GetExerciseProgressAsync(userId, SeededExerciseId, DateTime.UtcNow.AddMonths(-1), null);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetExerciseProgressAsync_ResultsAreTimeOrdered()
    {
        var sut = CreateSut(out var dbContext);
        var userId = Guid.NewGuid();
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-1), (10, 20m));
        AddCompletedSession(dbContext, userId, DateTime.UtcNow.AddDays(-5), (10, 18m));

        var result = await sut.GetExerciseProgressAsync(userId, SeededExerciseId, null, null);

        Assert.True(result[0].SessionDate < result[1].SessionDate);
    }
}
