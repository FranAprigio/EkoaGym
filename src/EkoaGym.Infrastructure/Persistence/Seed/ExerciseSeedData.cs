using EkoaGym.Domain.Entities;
using EkoaGym.Domain.Enums;

namespace EkoaGym.Infrastructure.Persistence.Seed;

// Original starter content (not sourced from WGER/ExerciseDB) to sidestep external-dataset licensing
// concerns for the MVP; ImageUrl points at the uploads/exercises/ convention until real assets are added.
public static class ExerciseSeedData
{
    private static readonly DateTime SeededAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Exercise[] All =
    [
        Create("10000000-0000-0000-0000-000000000001", "Barbell Bench Press", MuscleGroup.Chest, Equipment.Barbell, Difficulty.Beginner,
            "Lie on a flat bench, lower the bar to mid-chest with control, then press it back up to full arm extension."),
        Create("10000000-0000-0000-0000-000000000002", "Incline Dumbbell Press", MuscleGroup.Chest, Equipment.Dumbbell, Difficulty.Beginner,
            "On an inclined bench, press two dumbbells from shoulder height to full extension, targeting the upper chest."),
        Create("10000000-0000-0000-0000-000000000003", "Push-Up", MuscleGroup.Chest, Equipment.Bodyweight, Difficulty.Beginner,
            "Keep your body in a straight line and lower your chest to the floor, then push back up to full arm extension."),
        Create("10000000-0000-0000-0000-000000000004", "Cable Chest Fly", MuscleGroup.Chest, Equipment.Cable, Difficulty.Intermediate,
            "With cables set at chest height, bring the handles together in front of you with a slight bend in the elbows."),

        Create("10000000-0000-0000-0000-000000000005", "Deadlift", MuscleGroup.Back, Equipment.Barbell, Difficulty.Intermediate,
            "Hinge at the hips to grip the bar, then drive through the floor to stand tall, keeping the bar close to your body."),
        Create("10000000-0000-0000-0000-000000000006", "Pull-Up", MuscleGroup.Back, Equipment.PullUpBar, Difficulty.Intermediate,
            "Hang from a bar with palms facing away, then pull your chin above the bar and lower back down with control."),
        Create("10000000-0000-0000-0000-000000000007", "Bent-Over Barbell Row", MuscleGroup.Back, Equipment.Barbell, Difficulty.Intermediate,
            "Hinge forward with a flat back and row the bar toward your lower ribs, squeezing your shoulder blades together."),
        Create("10000000-0000-0000-0000-000000000008", "Lat Pulldown", MuscleGroup.Back, Equipment.Machine, Difficulty.Beginner,
            "Pull the bar down to upper chest level while keeping your torso upright, then control it back to full extension."),

        Create("10000000-0000-0000-0000-000000000009", "Overhead Barbell Press", MuscleGroup.Shoulders, Equipment.Barbell, Difficulty.Intermediate,
            "Press the bar from shoulder height straight overhead until your arms are fully extended, then lower with control."),
        Create("10000000-0000-0000-0000-00000000000a", "Dumbbell Lateral Raise", MuscleGroup.Shoulders, Equipment.Dumbbell, Difficulty.Beginner,
            "Raise two dumbbells out to the sides until arms are roughly parallel to the floor, then lower slowly."),
        Create("10000000-0000-0000-0000-00000000000b", "Face Pull", MuscleGroup.Shoulders, Equipment.Cable, Difficulty.Beginner,
            "Pull a rope attachment toward your face at eye height, flaring your elbows out to target the rear delts."),
        Create("10000000-0000-0000-0000-00000000000c", "Arnold Press", MuscleGroup.Shoulders, Equipment.Dumbbell, Difficulty.Intermediate,
            "Start with palms facing you at shoulder height, rotate outward while pressing the dumbbells overhead."),

        Create("10000000-0000-0000-0000-00000000000d", "Barbell Curl", MuscleGroup.Biceps, Equipment.Barbell, Difficulty.Beginner,
            "Curl the bar from thigh height to shoulder height, keeping elbows fixed at your sides throughout."),
        Create("10000000-0000-0000-0000-00000000000e", "Dumbbell Hammer Curl", MuscleGroup.Biceps, Equipment.Dumbbell, Difficulty.Beginner,
            "Curl dumbbells with palms facing each other throughout the movement to emphasize the brachialis."),
        Create("10000000-0000-0000-0000-00000000000f", "Cable Curl", MuscleGroup.Biceps, Equipment.Cable, Difficulty.Beginner,
            "Using a straight bar attachment, curl the cable up to shoulder height while keeping elbows stationary."),

        Create("10000000-0000-0000-0000-000000000010", "Triceps Dip", MuscleGroup.Triceps, Equipment.Bodyweight, Difficulty.Intermediate,
            "Lower your body between parallel bars until elbows reach roughly 90 degrees, then press back up."),
        Create("10000000-0000-0000-0000-000000000011", "Cable Triceps Pushdown", MuscleGroup.Triceps, Equipment.Cable, Difficulty.Beginner,
            "Push a bar or rope attachment down until arms are fully extended, keeping elbows tucked at your sides."),
        Create("10000000-0000-0000-0000-000000000012", "Skull Crusher", MuscleGroup.Triceps, Equipment.Barbell, Difficulty.Intermediate,
            "Lying on a bench, lower the bar toward your forehead by bending only at the elbows, then extend back up."),

        Create("10000000-0000-0000-0000-000000000013", "Wrist Curl", MuscleGroup.Forearms, Equipment.Dumbbell, Difficulty.Beginner,
            "Resting forearms on your thighs or a bench, curl the dumbbell upward using only your wrists."),
        Create("10000000-0000-0000-0000-000000000014", "Farmer's Carry", MuscleGroup.Forearms, Equipment.Dumbbell, Difficulty.Beginner,
            "Carry a heavy dumbbell or kettlebell in each hand for a set distance while keeping your posture upright."),

        Create("10000000-0000-0000-0000-000000000015", "Plank", MuscleGroup.Abs, Equipment.Bodyweight, Difficulty.Beginner,
            "Hold a straight-line position on your forearms and toes, keeping your core braced and hips level."),
        Create("10000000-0000-0000-0000-000000000016", "Hanging Leg Raise", MuscleGroup.Abs, Equipment.PullUpBar, Difficulty.Advanced,
            "Hang from a bar and raise your legs to hip height or higher without swinging, then lower with control."),
        Create("10000000-0000-0000-0000-000000000017", "Cable Crunch", MuscleGroup.Abs, Equipment.Cable, Difficulty.Intermediate,
            "Kneeling below a high cable, crunch your torso down toward your knees by flexing the abs, not the hips."),
        Create("10000000-0000-0000-0000-000000000018", "Russian Twist", MuscleGroup.Abs, Equipment.MedicineBall, Difficulty.Beginner,
            "Sit with knees bent and torso leaned back slightly, rotating a weight from side to side across your body."),

        Create("10000000-0000-0000-0000-000000000019", "Barbell Back Squat", MuscleGroup.Quadriceps, Equipment.Barbell, Difficulty.Intermediate,
            "With the bar racked on your upper back, squat down until thighs are at least parallel to the floor, then stand."),
        Create("10000000-0000-0000-0000-00000000001a", "Leg Press", MuscleGroup.Quadriceps, Equipment.Machine, Difficulty.Beginner,
            "Push the platform away by extending your knees and hips, then lower it back under control."),
        Create("10000000-0000-0000-0000-00000000001b", "Walking Lunge", MuscleGroup.Quadriceps, Equipment.Dumbbell, Difficulty.Beginner,
            "Step forward into a lunge, lowering the back knee toward the floor, then bring the trailing leg through to the next step."),
        Create("10000000-0000-0000-0000-00000000001c", "Leg Extension", MuscleGroup.Quadriceps, Equipment.Machine, Difficulty.Beginner,
            "Extend your knees against the machine's resistance pad until legs are straight, then lower with control."),

        Create("10000000-0000-0000-0000-00000000001d", "Romanian Deadlift", MuscleGroup.Hamstrings, Equipment.Barbell, Difficulty.Intermediate,
            "With a slight knee bend, hinge at the hips to lower the bar along your legs, feeling a stretch in the hamstrings."),
        Create("10000000-0000-0000-0000-00000000001e", "Leg Curl", MuscleGroup.Hamstrings, Equipment.Machine, Difficulty.Beginner,
            "Curl the machine's pad toward your glutes by flexing at the knee, then lower back to the start position."),
        Create("10000000-0000-0000-0000-00000000001f", "Good Morning", MuscleGroup.Hamstrings, Equipment.Barbell, Difficulty.Advanced,
            "With the bar on your upper back, hinge forward at the hips keeping a flat back, then return to standing."),

        Create("10000000-0000-0000-0000-000000000020", "Hip Thrust", MuscleGroup.Glutes, Equipment.Barbell, Difficulty.Intermediate,
            "With shoulders on a bench and the bar over your hips, drive through your heels to lift your hips to full extension."),
        Create("10000000-0000-0000-0000-000000000021", "Glute Bridge", MuscleGroup.Glutes, Equipment.Bodyweight, Difficulty.Beginner,
            "Lying on your back with knees bent, drive through your heels to raise your hips, squeezing the glutes at the top."),
        Create("10000000-0000-0000-0000-000000000022", "Cable Kickback", MuscleGroup.Glutes, Equipment.Cable, Difficulty.Beginner,
            "With an ankle cuff attached to a low cable, kick your leg straight back while keeping your torso stable."),

        Create("10000000-0000-0000-0000-000000000023", "Standing Calf Raise", MuscleGroup.Calves, Equipment.Machine, Difficulty.Beginner,
            "Rise onto the balls of your feet as high as possible, then lower your heels below the platform for a full stretch."),
        Create("10000000-0000-0000-0000-000000000024", "Seated Calf Raise", MuscleGroup.Calves, Equipment.Machine, Difficulty.Beginner,
            "With knees bent under the pad, raise your heels by extending your ankles, then lower back down slowly."),

        Create("10000000-0000-0000-0000-000000000025", "Kettlebell Swing", MuscleGroup.FullBody, Equipment.Kettlebell, Difficulty.Intermediate,
            "Hinge at the hips to swing the kettlebell between your legs, then snap your hips forward to drive it to chest height."),
        Create("10000000-0000-0000-0000-000000000026", "Burpee", MuscleGroup.FullBody, Equipment.Bodyweight, Difficulty.Intermediate,
            "From standing, drop into a push-up, then explosively return to standing and jump, repeating in one fluid motion."),
        Create("10000000-0000-0000-0000-000000000027", "Clean and Press", MuscleGroup.FullBody, Equipment.Barbell, Difficulty.Advanced,
            "Explosively pull the bar from the floor to your shoulders, then press it overhead to full arm extension."),

        Create("10000000-0000-0000-0000-000000000028", "Treadmill Running", MuscleGroup.Cardio, Equipment.Other, Difficulty.Beginner,
            "Maintain a steady running pace on the treadmill, adjusting speed and incline to control intensity."),
        Create("10000000-0000-0000-0000-000000000029", "Rowing Machine", MuscleGroup.Cardio, Equipment.Machine, Difficulty.Beginner,
            "Drive with your legs, then lean back and pull the handle to your ribs, reversing the sequence on the return."),
        Create("10000000-0000-0000-0000-00000000002a", "Jump Rope", MuscleGroup.Cardio, Equipment.Other, Difficulty.Beginner,
            "Turn the rope with your wrists and jump just high enough to clear it, keeping a steady, light rhythm."),
    ];

    private static Exercise Create(string id, string name, MuscleGroup muscleGroup, Equipment equipment, Difficulty difficulty, string instructionText) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        MuscleGroup = muscleGroup,
        Equipment = equipment,
        Difficulty = difficulty,
        ImageUrl = $"/uploads/exercises/{Slugify(name)}.gif",
        InstructionText = instructionText,
        CreatedAt = SeededAt,
        UpdatedAt = SeededAt,
        IntensityFactor = DefaultIntensityFactor(name)
    };

    // M-10.2: heavy compound barbell lifts score at 1.2x, small-muscle isolation/accessory work at
    // 0.8x, everything else (bodyweight compounds, cardio, core) at the 1.0x default.
    private static decimal DefaultIntensityFactor(string name)
    {
        string[] isolationKeywords = ["Curl", "Raise", "Fly", "Pushdown", "Kickback", "Extension", "Face Pull", "Skull Crusher"];
        if (isolationKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            return 0.8m;
        }

        string[] compoundKeywords = ["Bench Press", "Deadlift", "Squat", "Overhead", "Row", "Hip Thrust", "Clean and Press", "Good Morning"];
        if (compoundKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            return 1.2m;
        }

        return 1.0m;
    }

    private static string Slugify(string name) => name.ToLowerInvariant().Replace(" ", "-").Replace("'", "");
}
