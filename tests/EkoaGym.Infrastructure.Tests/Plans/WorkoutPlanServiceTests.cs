using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Plans;
using EkoaGym.Infrastructure.Persistence.Seed;
using EkoaGym.Infrastructure.Plans;

namespace EkoaGym.Infrastructure.Tests.Plans;

public class WorkoutPlanServiceTests
{
    private static readonly Guid SeededExerciseId = ExerciseSeedData.All[0].Id;

    private static WorkoutPlanService CreateSut(out Persistence.EkoaGymDbContext dbContext)
    {
        dbContext = TestDbContextFactory.Create();
        return new WorkoutPlanService(dbContext);
    }

    private static CreatePlanRequest ValidCreateRequest() => new(
        "Push Day",
        [new PlanExerciseRequest(SeededExerciseId, 3, 10, 20, 90)]);

    [Fact]
    public async Task CreatePlanAsync_ValidRequest_CreatesPlanWithOrderedExercises()
    {
        var sut = CreateSut(out _);
        var userId = Guid.NewGuid();

        var plan = await sut.CreatePlanAsync(userId, ValidCreateRequest());

        Assert.Equal("Push Day", plan.Name);
        Assert.Single(plan.Exercises);
        Assert.Equal(0, plan.Exercises[0].OrderIndex);
    }

    [Fact]
    public async Task CreatePlanAsync_UnknownExercise_ThrowsExerciseNotFoundException()
    {
        var sut = CreateSut(out _);
        var request = new CreatePlanRequest("Bad Plan", [new PlanExerciseRequest(Guid.NewGuid(), 3, 10, 20, 90)]);

        await Assert.ThrowsAsync<ExerciseNotFoundException>(() => sut.CreatePlanAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task GetPlanAsync_UnknownPlan_ThrowsPlanNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<PlanNotFoundException>(() => sut.GetPlanAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetPlanAsync_AnotherUsersPlan_ThrowsPlanAccessDeniedException()
    {
        var sut = CreateSut(out _);
        var ownerId = Guid.NewGuid();
        var plan = await sut.CreatePlanAsync(ownerId, ValidCreateRequest());

        await Assert.ThrowsAsync<PlanAccessDeniedException>(() => sut.GetPlanAsync(Guid.NewGuid(), plan.Id));
    }

    [Fact]
    public async Task DeletePlanAsync_SoftDeletes_PlanNoLongerRetrievable()
    {
        var sut = CreateSut(out _);
        var userId = Guid.NewGuid();
        var plan = await sut.CreatePlanAsync(userId, ValidCreateRequest());

        await sut.DeletePlanAsync(userId, plan.Id);

        await Assert.ThrowsAsync<PlanNotFoundException>(() => sut.GetPlanAsync(userId, plan.Id));
    }

    [Fact]
    public async Task DuplicatePlanAsync_AppendsCopySuffixAndCopiesExercises()
    {
        var sut = CreateSut(out _);
        var userId = Guid.NewGuid();
        var plan = await sut.CreatePlanAsync(userId, ValidCreateRequest());

        var duplicate = await sut.DuplicatePlanAsync(userId, plan.Id);

        Assert.Equal("Push Day (copy)", duplicate.Name);
        Assert.NotEqual(plan.Id, duplicate.Id);
        Assert.Single(duplicate.Exercises);
    }

    [Fact]
    public async Task UpdatePlanAsync_ReplacesExerciseList()
    {
        var sut = CreateSut(out _);
        var userId = Guid.NewGuid();
        var plan = await sut.CreatePlanAsync(userId, ValidCreateRequest());

        var updateRequest = new UpdatePlanRequest("Push Day v2", [new PlanExerciseRequest(SeededExerciseId, 5, 5, 40, 120)]);
        var updated = await sut.UpdatePlanAsync(userId, plan.Id, updateRequest);

        Assert.Equal("Push Day v2", updated.Name);
        Assert.Equal(5, updated.Exercises[0].Sets);
        Assert.Equal(40, updated.Exercises[0].WeightKg);
    }
}
