using EkoaGym.Mobile.Models;

namespace EkoaGym.Mobile.Services.Scheduling;

// Deterministic date -> plan assignment computed purely client-side (no backend "schedule" entity exists,
// and none is being added — Home/Schedule reuse this so the same date always maps to the same plan).
public static class PlanScheduler
{
    public static PlanSummary? GetPlanForDate(IReadOnlyList<PlanSummary> plans, DateOnly date)
    {
        if (plans.Count == 0)
        {
            return null;
        }

        var index = ((date.DayNumber % plans.Count) + plans.Count) % plans.Count;
        return plans[index];
    }
}
