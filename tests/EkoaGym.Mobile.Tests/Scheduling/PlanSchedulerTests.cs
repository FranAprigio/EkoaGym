using EkoaGym.Mobile.Models;
using EkoaGym.Mobile.Services.Scheduling;

namespace EkoaGym.Mobile.Tests.Scheduling;

public class PlanSchedulerTests
{
    private static PlanSummary Plan(string name) =>
        new(Guid.NewGuid(), name, ExerciseCount: 1, CreatedAt: DateTime.UtcNow, UpdatedAt: DateTime.UtcNow);

    [Fact]
    public void GetPlanForDate_EmptyList_ReturnsNull()
    {
        var result = PlanScheduler.GetPlanForDate([], new DateOnly(2026, 8, 21));

        Assert.Null(result);
    }

    [Fact]
    public void GetPlanForDate_SamePlanListAndDate_IsDeterministic()
    {
        var plans = new List<PlanSummary> { Plan("A"), Plan("B"), Plan("C") };
        var date = new DateOnly(2026, 8, 21);

        var first = PlanScheduler.GetPlanForDate(plans, date);
        var second = PlanScheduler.GetPlanForDate(plans, date);

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetPlanForDate_ConsecutiveDates_CycleThroughAllPlans()
    {
        var plans = new List<PlanSummary> { Plan("A"), Plan("B"), Plan("C") };
        var start = new DateOnly(2026, 8, 21);

        var assigned = Enumerable.Range(0, plans.Count)
            .Select(offset => PlanScheduler.GetPlanForDate(plans, start.AddDays(offset)))
            .ToList();

        Assert.Equal(plans.Count, assigned.Distinct().Count());
    }

    [Fact]
    public void GetPlanForDate_SinglePlan_AlwaysReturnsThatPlan()
    {
        var plan = Plan("Only");

        var result = PlanScheduler.GetPlanForDate([plan], new DateOnly(1, 1, 1));

        Assert.Equal(plan, result);
    }
}
