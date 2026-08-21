namespace EkoaGym.Application.BodyMetrics;

public interface IBodyMetricService
{
    Task<IReadOnlyList<BodyMetricResponse>> GetBodyMetricsAsync(Guid userId, BodyMetricFilter filter, CancellationToken cancellationToken = default);
    Task<BodyMetricResponse> CreateBodyMetricAsync(Guid userId, CreateBodyMetricRequest request, CancellationToken cancellationToken = default);
}
