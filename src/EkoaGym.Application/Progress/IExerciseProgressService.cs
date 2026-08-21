namespace EkoaGym.Application.Progress;

public interface IExerciseProgressService
{
    Task<IReadOnlyList<ExerciseProgressPoint>> GetExerciseProgressAsync(
        Guid userId, Guid exerciseId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
