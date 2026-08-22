using EkoaGym.Domain.Enums;

namespace EkoaGym.Infrastructure.ExerciseLimits;

// Shared by ExerciseLimitService (BE-7.3, the full list) and WorkoutSessionService (the single-exercise
// server-side enforcement on BE-5.2) so the two never drift apart.
public static class ExerciseLimitDefaults
{
    private static readonly IReadOnlyDictionary<MuscleGroup, decimal> DefaultCapsByMuscleGroup = new Dictionary<MuscleGroup, decimal>
    {
        [MuscleGroup.Chest] = 300m,
        [MuscleGroup.Back] = 300m,
        [MuscleGroup.Shoulders] = 150m,
        [MuscleGroup.Biceps] = 120m,
        [MuscleGroup.Triceps] = 150m,
        [MuscleGroup.Forearms] = 100m,
        [MuscleGroup.Abs] = 100m,
        [MuscleGroup.Quadriceps] = 500m,
        [MuscleGroup.Hamstrings] = 400m,
        [MuscleGroup.Glutes] = 400m,
        [MuscleGroup.Calves] = 300m,
        [MuscleGroup.FullBody] = 200m,
        [MuscleGroup.Cardio] = 50m
    };

    private const decimal FallbackCap = 300m;

    public static decimal GetDefaultCap(MuscleGroup muscleGroup) => DefaultCapsByMuscleGroup.GetValueOrDefault(muscleGroup, FallbackCap);
}
