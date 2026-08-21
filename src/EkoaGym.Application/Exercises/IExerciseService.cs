using EkoaGym.Application.Common;

namespace EkoaGym.Application.Exercises;

public interface IExerciseService
{
    Task<PagedResult<ExerciseSummaryResponse>> GetExercisesAsync(ExerciseFilter filter, CancellationToken cancellationToken = default);
    Task<ExerciseDetailResponse> GetExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
