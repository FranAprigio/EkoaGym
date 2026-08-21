namespace EkoaGym.Application.Plans;

public interface IWorkoutPlanService
{
    Task<IReadOnlyList<PlanSummaryResponse>> GetPlansAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PlanDetailResponse> GetPlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
    Task<PlanDetailResponse> CreatePlanAsync(Guid userId, CreatePlanRequest request, CancellationToken cancellationToken = default);
    Task<PlanDetailResponse> UpdatePlanAsync(Guid userId, Guid planId, UpdatePlanRequest request, CancellationToken cancellationToken = default);
    Task DeletePlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
    Task<PlanDetailResponse> DuplicatePlanAsync(Guid userId, Guid planId, CancellationToken cancellationToken = default);
}
