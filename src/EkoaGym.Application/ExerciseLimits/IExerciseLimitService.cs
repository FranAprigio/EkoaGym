namespace EkoaGym.Application.ExerciseLimits;

public interface IExerciseLimitService
{
    Task<IReadOnlyList<ExerciseLimitResponse>> GetLimitsAsync(CancellationToken cancellationToken = default);
}
