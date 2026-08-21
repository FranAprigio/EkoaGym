namespace EkoaGym.Application.Plans;

public interface IProgressionService
{
    Task<NextTargetResponse> GetNextTargetAsync(Guid userId, Guid planId, Guid exerciseId, CancellationToken cancellationToken = default);
}
