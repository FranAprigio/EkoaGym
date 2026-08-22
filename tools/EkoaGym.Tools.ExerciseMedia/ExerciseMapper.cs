namespace EkoaGym.Tools.ExerciseMedia;

// Keyword-based mapping from the dataset's free-text equipment/target strings onto EkoaGym's fixed
// enums. The dataset uses ~27 equipment values and 19 target-muscle values that don't line up 1:1 with
// EkoaGym.Domain.Enums.Equipment (11 values) / MuscleGroup (13 values), so this is a best-effort bucket
// classification, not an exact translation. Spot-check the "Other" equipment fallback after generating.
public static class ExerciseMapper
{
    public static string MapEquipment(string equipment) => equipment.Trim().ToLowerInvariant() switch
    {
        "body weight" => "Bodyweight",
        "barbell" or "ez barbell" or "olympic barbell" or "trap bar" => "Barbell",
        "dumbbell" => "Dumbbell",
        "kettlebell" => "Kettlebell",
        "cable" => "Cable",
        "band" or "resistance band" => "ResistanceBand",
        "medicine ball" => "MedicineBall",
        "assisted" or "leverage machine" or "smith machine" or "sled machine" or "elliptical machine"
            or "skierg machine" or "stepmill machine" or "stationary bike" or "upper body ergometer"
            or "hammer" => "Machine",
        _ => "Other", // bosu ball, roller, rope, stability ball, tire, weighted, wheel roller
    };

    public static string MapMuscleGroup(string target) => target.Trim().ToLowerInvariant() switch
    {
        "pectorals" => "Chest",
        "lats" or "upper back" or "spine" or "traps" or "levator scapulae" => "Back",
        "delts" => "Shoulders",
        "biceps" => "Biceps",
        "triceps" => "Triceps",
        "forearms" => "Forearms",
        "abs" => "Abs",
        "quads" or "adductors" => "Quadriceps",
        "hamstrings" => "Hamstrings",
        "glutes" or "abductors" => "Glutes",
        "calves" => "Calves",
        "serratus anterior" => "Chest",
        "cardiovascular system" => "Cardio",
        _ => "FullBody",
    };

    // No "difficulty" concept exists in the source dataset — derive one from equipment/name keywords,
    // following the same pragmatic heuristic pattern as the existing DefaultIntensityFactor helper.
    public static string DefaultDifficulty(string name, string equipmentRaw)
    {
        var n = name.ToLowerInvariant();
        var eq = equipmentRaw.Trim().ToLowerInvariant();

        if (n.Contains("deadlift") || n.Contains("snatch") || n.Contains("clean") || n.Contains("muscle-up")
            || n.Contains("muscle up") || n.Contains("pistol") || n.Contains("handstand") || n.Contains("olympic"))
        {
            return "Advanced";
        }

        if (eq is "body weight" or "assisted" or "elliptical machine" or "stationary bike" or "skierg machine"
            or "stepmill machine" or "upper body ergometer" or "leverage machine" or "smith machine")
        {
            return "Beginner";
        }

        return "Intermediate";
    }

    // Same 1.2/1.0/0.8 compound-vs-isolation heuristic already used for the original 42 seed exercises
    // (see Infrastructure/Persistence/Seed/ExerciseSeedData.cs) — reapplied here for consistency.
    public static decimal DefaultIntensityFactor(string name)
    {
        var n = name.ToLowerInvariant();
        string[] compoundKeywords = ["squat", "deadlift", "bench press", "overhead press", "row", "pull-up", "pull up", "clean", "snatch", "press"];
        string[] isolationKeywords = ["curl", "extension", "raise", "fly", "kickback", "pushdown", "crunch"];

        if (compoundKeywords.Any(k => n.Contains(k)))
        {
            return 1.2m;
        }

        if (isolationKeywords.Any(k => n.Contains(k)))
        {
            return 0.8m;
        }

        return 1.0m;
    }
}
