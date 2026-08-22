using EkoaGym.Domain.Entities;

namespace EkoaGym.Infrastructure.Persistence.Seed;

// Hard caps from the M-10.3 spec for the 4 named lifts; every other exercise falls back to a
// per-muscle-group default computed at read time in ExerciseLimitService (not seeded here).
public static class ExerciseLimitSeedData
{
    public static readonly ExerciseLimit[] All =
    [
        Create("20000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000001", 350m), // Barbell Bench Press
        Create("20000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000005", 500m), // Deadlift
        Create("20000000-0000-0000-0000-000000000003", "10000000-0000-0000-0000-000000000019", 500m), // Barbell Back Squat
        Create("20000000-0000-0000-0000-000000000004", "10000000-0000-0000-0000-00000000000d", 120m), // Barbell Curl
    ];

    private static ExerciseLimit Create(string id, string exerciseId, decimal maxWeightKg) => new()
    {
        Id = Guid.Parse(id),
        ExerciseId = Guid.Parse(exerciseId),
        MaxWeightKg = maxWeightKg
    };
}
