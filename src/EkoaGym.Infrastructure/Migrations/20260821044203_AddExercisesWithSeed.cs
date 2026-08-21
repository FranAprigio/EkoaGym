using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EkoaGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExercisesWithSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MuscleGroup = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstructionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "CreatedAt", "Difficulty", "Equipment", "ImageUrl", "InstructionText", "MuscleGroup", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Barbell", "/uploads/exercises/barbell-bench-press.gif", "Lie on a flat bench, lower the bar to mid-chest with control, then press it back up to full arm extension.", "Chest", "Barbell Bench Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/incline-dumbbell-press.gif", "On an inclined bench, press two dumbbells from shoulder height to full extension, targeting the upper chest.", "Chest", "Incline Dumbbell Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Bodyweight", "/uploads/exercises/push-up.gif", "Keep your body in a straight line and lower your chest to the floor, then push back up to full arm extension.", "Chest", "Push-Up", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Cable", "/uploads/exercises/cable-chest-fly.gif", "With cables set at chest height, bring the handles together in front of you with a slight bend in the elbows.", "Chest", "Cable Chest Fly", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/deadlift.gif", "Hinge at the hips to grip the bar, then drive through the floor to stand tall, keeping the bar close to your body.", "Back", "Deadlift", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "PullUpBar", "/uploads/exercises/pull-up.gif", "Hang from a bar with palms facing away, then pull your chin above the bar and lower back down with control.", "Back", "Pull-Up", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/bent-over-barbell-row.gif", "Hinge forward with a flat back and row the bar toward your lower ribs, squeezing your shoulder blades together.", "Back", "Bent-Over Barbell Row", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/lat-pulldown.gif", "Pull the bar down to upper chest level while keeping your torso upright, then control it back to full extension.", "Back", "Lat Pulldown", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/overhead-barbell-press.gif", "Press the bar from shoulder height straight overhead until your arms are fully extended, then lower with control.", "Shoulders", "Overhead Barbell Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/dumbbell-lateral-raise.gif", "Raise two dumbbells out to the sides until arms are roughly parallel to the floor, then lower slowly.", "Shoulders", "Dumbbell Lateral Raise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Cable", "/uploads/exercises/face-pull.gif", "Pull a rope attachment toward your face at eye height, flaring your elbows out to target the rear delts.", "Shoulders", "Face Pull", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Dumbbell", "/uploads/exercises/arnold-press.gif", "Start with palms facing you at shoulder height, rotate outward while pressing the dumbbells overhead.", "Shoulders", "Arnold Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Barbell", "/uploads/exercises/barbell-curl.gif", "Curl the bar from thigh height to shoulder height, keeping elbows fixed at your sides throughout.", "Biceps", "Barbell Curl", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/dumbbell-hammer-curl.gif", "Curl dumbbells with palms facing each other throughout the movement to emphasize the brachialis.", "Biceps", "Dumbbell Hammer Curl", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Cable", "/uploads/exercises/cable-curl.gif", "Using a straight bar attachment, curl the cable up to shoulder height while keeping elbows stationary.", "Biceps", "Cable Curl", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Bodyweight", "/uploads/exercises/triceps-dip.gif", "Lower your body between parallel bars until elbows reach roughly 90 degrees, then press back up.", "Triceps", "Triceps Dip", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Cable", "/uploads/exercises/cable-triceps-pushdown.gif", "Push a bar or rope attachment down until arms are fully extended, keeping elbows tucked at your sides.", "Triceps", "Cable Triceps Pushdown", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/skull-crusher.gif", "Lying on a bench, lower the bar toward your forehead by bending only at the elbows, then extend back up.", "Triceps", "Skull Crusher", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000013"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/wrist-curl.gif", "Resting forearms on your thighs or a bench, curl the dumbbell upward using only your wrists.", "Forearms", "Wrist Curl", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000014"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/farmers-carry.gif", "Carry a heavy dumbbell or kettlebell in each hand for a set distance while keeping your posture upright.", "Forearms", "Farmer's Carry", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000015"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Bodyweight", "/uploads/exercises/plank.gif", "Hold a straight-line position on your forearms and toes, keeping your core braced and hips level.", "Abs", "Plank", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000016"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", "PullUpBar", "/uploads/exercises/hanging-leg-raise.gif", "Hang from a bar and raise your legs to hip height or higher without swinging, then lower with control.", "Abs", "Hanging Leg Raise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000017"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Cable", "/uploads/exercises/cable-crunch.gif", "Kneeling below a high cable, crunch your torso down toward your knees by flexing the abs, not the hips.", "Abs", "Cable Crunch", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000018"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "MedicineBall", "/uploads/exercises/russian-twist.gif", "Sit with knees bent and torso leaned back slightly, rotating a weight from side to side across your body.", "Abs", "Russian Twist", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000019"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/barbell-back-squat.gif", "With the bar racked on your upper back, squat down until thighs are at least parallel to the floor, then stand.", "Quadriceps", "Barbell Back Squat", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/leg-press.gif", "Push the platform away by extending your knees and hips, then lower it back under control.", "Quadriceps", "Leg Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Dumbbell", "/uploads/exercises/walking-lunge.gif", "Step forward into a lunge, lowering the back knee toward the floor, then bring the trailing leg through to the next step.", "Quadriceps", "Walking Lunge", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/leg-extension.gif", "Extend your knees against the machine's resistance pad until legs are straight, then lower with control.", "Quadriceps", "Leg Extension", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/romanian-deadlift.gif", "With a slight knee bend, hinge at the hips to lower the bar along your legs, feeling a stretch in the hamstrings.", "Hamstrings", "Romanian Deadlift", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/leg-curl.gif", "Curl the machine's pad toward your glutes by flexing at the knee, then lower back to the start position.", "Hamstrings", "Leg Curl", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000001f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", "Barbell", "/uploads/exercises/good-morning.gif", "With the bar on your upper back, hinge forward at the hips keeping a flat back, then return to standing.", "Hamstrings", "Good Morning", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000020"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Barbell", "/uploads/exercises/hip-thrust.gif", "With shoulders on a bench and the bar over your hips, drive through your heels to lift your hips to full extension.", "Glutes", "Hip Thrust", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000021"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Bodyweight", "/uploads/exercises/glute-bridge.gif", "Lying on your back with knees bent, drive through your heels to raise your hips, squeezing the glutes at the top.", "Glutes", "Glute Bridge", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000022"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Cable", "/uploads/exercises/cable-kickback.gif", "With an ankle cuff attached to a low cable, kick your leg straight back while keeping your torso stable.", "Glutes", "Cable Kickback", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000023"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/standing-calf-raise.gif", "Rise onto the balls of your feet as high as possible, then lower your heels below the platform for a full stretch.", "Calves", "Standing Calf Raise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000024"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/seated-calf-raise.gif", "With knees bent under the pad, raise your heels by extending your ankles, then lower back down slowly.", "Calves", "Seated Calf Raise", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000025"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Kettlebell", "/uploads/exercises/kettlebell-swing.gif", "Hinge at the hips to swing the kettlebell between your legs, then snap your hips forward to drive it to chest height.", "FullBody", "Kettlebell Swing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000026"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Intermediate", "Bodyweight", "/uploads/exercises/burpee.gif", "From standing, drop into a push-up, then explosively return to standing and jump, repeating in one fluid motion.", "FullBody", "Burpee", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000027"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced", "Barbell", "/uploads/exercises/clean-and-press.gif", "Explosively pull the bar from the floor to your shoulders, then press it overhead to full arm extension.", "FullBody", "Clean and Press", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000028"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Other", "/uploads/exercises/treadmill-running.gif", "Maintain a steady running pace on the treadmill, adjusting speed and incline to control intensity.", "Cardio", "Treadmill Running", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000029"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Machine", "/uploads/exercises/rowing-machine.gif", "Drive with your legs, then lean back and pull the handle to your ribs, reversing the sequence on the return.", "Cardio", "Rowing Machine", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000002a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beginner", "Other", "/uploads/exercises/jump-rope.gif", "Turn the rope with your wrists and jump just high enough to clear it, keeping a steady, light rhythm.", "Cardio", "Jump Rope", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Difficulty",
                table: "Exercises",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Equipment",
                table: "Exercises",
                column: "Equipment");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_MuscleGroup",
                table: "Exercises",
                column: "MuscleGroup");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
