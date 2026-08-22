using System.Text;
using System.Text.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace EkoaGym.Tools.ExerciseMedia;

// One-time utility (not part of the running app) that:
//   1. "upload"       — bulk-uploads the exercises-dataset images/ + videos/ folders to Cloudinary.
//   2. "generate-seed" — regenerates ExerciseSeedData.cs from data/exercises.json + the upload's media map.
//
// Usage:
//   dotnet run -- upload --dataset <path-to-exercises-dataset-main> --media-map media-map.json
//   dotnet run -- generate-seed --dataset <path-to-exercises-dataset-main> --media-map media-map.json --output <path-to-ExerciseSeedData.cs>
//
// "upload" requires the CLOUDINARY_URL environment variable (cloudinary://<api_key>:<api_secret>@<cloud_name>)
// to be set in your OWN shell before running this — never pass credentials as a command-line argument or
// paste them into chat/config files that get committed.
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var options = ParseOptions(args[1..]);

        switch (args[0])
        {
            case "upload":
                return await UploadAsync(options);
            case "generate-seed":
                return GenerateSeed(options);
            default:
                PrintUsage();
                return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- upload --dataset <path> --media-map <path>");
        Console.WriteLine("  dotnet run -- generate-seed --dataset <path> --media-map <path> --output <path>");
    }

    private static Dictionary<string, string> ParseOptions(string[] args)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i + 1 < args.Length; i += 2)
        {
            result[args[i].TrimStart('-')] = args[i + 1];
        }

        return result;
    }

    private static async Task<int> UploadAsync(Dictionary<string, string> options)
    {
        if (!options.TryGetValue("dataset", out var datasetRoot) || !options.TryGetValue("media-map", out var mediaMapPath))
        {
            PrintUsage();
            return 1;
        }

        if (Environment.GetEnvironmentVariable("CLOUDINARY_URL") is null)
        {
            Console.Error.WriteLine("CLOUDINARY_URL environment variable is not set. Set it in your own shell first, e.g.:");
            Console.Error.WriteLine("  $env:CLOUDINARY_URL = 'cloudinary://<api_key>:<api_secret>@<cloud_name>'");
            return 1;
        }

        var cloudinary = new Cloudinary(); // reads CLOUDINARY_URL automatically
        var account = cloudinary.Api.Account;
        if (string.IsNullOrWhiteSpace(account.Cloud) || string.IsNullOrWhiteSpace(account.ApiKey) || string.IsNullOrWhiteSpace(account.ApiSecret))
        {
            Console.Error.WriteLine("CLOUDINARY_URL is set but couldn't be parsed into cloud name/api key/api secret.");
            Console.Error.WriteLine("Expected format: cloudinary://<api_key>:<api_secret>@<cloud_name>");
            return 1;
        }

        Console.WriteLine($"Using Cloudinary account '{account.Cloud}'.");

        var exercises = LoadDataset(datasetRoot);
        var mediaMap = LoadMediaMap(mediaMapPath);

        Console.WriteLine($"Loaded {exercises.Count} dataset records; {mediaMap.Count} already uploaded.");

        var processed = 0;
        var consecutiveFailures = 0;
        foreach (var exercise in exercises)
        {
            if (mediaMap.ContainsKey(exercise.Id))
            {
                continue; // resumable: skip anything already uploaded from a previous run
            }

            // This dataset download only ships videos/ (the gifs) — no separate images/ thumbnail folder —
            // so the static thumbnail is derived from the gif's first frame at delivery time instead of
            // uploading a second file.
            var gifPath = Path.Combine(datasetRoot, exercise.GifUrl);
            if (!File.Exists(gifPath))
            {
                Console.Error.WriteLine($"Skipping {exercise.Id} ({exercise.Name}) — source file missing.");
                continue;
            }

            var gifPublicId = $"ekoagym/exercises/gif/{exercise.Id}";

            var gifResult = await cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(gifPath),
                PublicId = gifPublicId,
                UniqueFilename = false,
                Overwrite = false,
            });

            if (gifResult.Error is not null)
            {
                consecutiveFailures++;
                Console.Error.WriteLine($"Upload failed for {exercise.Id}: {gifResult.Error.Message}");

                if (processed == 0 && consecutiveFailures >= 3)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Aborting after 3 consecutive failures with zero successful uploads — this is almost");
                    Console.Error.WriteLine("always invalid Cloudinary credentials (wrong api_key/api_secret/cloud_name), not a");
                    Console.Error.WriteLine("per-file problem. Fix CLOUDINARY_URL and re-run (already-uploaded ids are skipped).");
                    return 1;
                }

                continue;
            }

            consecutiveFailures = 0;

            // pg_1 = first frame of the animated gif (a static preview); f_auto/q_auto picks the best
            // still-image format/compression for that frame. The full animation stays at gifResult.SecureUrl.
            var thumbnailUrl = $"https://res.cloudinary.com/{account.Cloud}/image/upload/pg_1,f_auto,q_auto/{gifPublicId}";

            mediaMap[exercise.Id] = new MediaMapEntry { ImageUrl = thumbnailUrl, GifUrl = gifResult.SecureUrl.ToString() };
            processed++;

            if (processed % 25 == 0)
            {
                SaveMediaMap(mediaMapPath, mediaMap); // flush periodically so an interruption doesn't lose progress
                Console.WriteLine($"Uploaded {processed}/{exercises.Count - mediaMap.Count + processed}...");
            }
        }

        SaveMediaMap(mediaMapPath, mediaMap);
        Console.WriteLine($"Done. {mediaMap.Count}/{exercises.Count} exercises have media uploaded.");
        return 0;
    }

    private static int GenerateSeed(Dictionary<string, string> options)
    {
        if (!options.TryGetValue("dataset", out var datasetRoot) || !options.TryGetValue("media-map", out var mediaMapPath)
            || !options.TryGetValue("output", out var outputPath))
        {
            PrintUsage();
            return 1;
        }

        var exercises = LoadDataset(datasetRoot);
        var mediaMap = LoadMediaMap(mediaMapPath);

        var missing = exercises.Where(e => !mediaMap.ContainsKey(e.Id)).ToList();
        if (missing.Count > 0)
        {
            Console.Error.WriteLine($"{missing.Count} exercises have no uploaded media yet (run 'upload' first). Aborting.");
            return 1;
        }

        var sb = new StringBuilder();
        sb.AppendLine("using EkoaGym.Domain.Entities;");
        sb.AppendLine("using EkoaGym.Domain.Enums;");
        sb.AppendLine();
        sb.AppendLine("namespace EkoaGym.Infrastructure.Persistence.Seed;");
        sb.AppendLine();
        sb.AppendLine("// Generated from https://github.com/hasaneyldrm/exercises-dataset (data: MIT license).");
        sb.AppendLine("// Media (thumbnail + animation gif) is (c) Gym visual, used under license and mirrored on");
        sb.AppendLine("// Cloudinary — every screen that shows this media must keep the \"(c) Gym visual\" attribution.");
        sb.AppendLine("// Regenerate via: dotnet run --project tools/EkoaGym.Tools.ExerciseMedia -- generate-seed ...");
        sb.AppendLine("public static class ExerciseSeedData");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly DateTime SeededAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);");
        sb.AppendLine();
        sb.AppendLine("    public static readonly Exercise[] All =");
        sb.AppendLine("    [");

        foreach (var exercise in exercises)
        {
            var media = mediaMap[exercise.Id];
            var guid = ToGuid(exercise.Id);
            var muscleGroup = ExerciseMapper.MapMuscleGroup(exercise.Target);
            var equipment = ExerciseMapper.MapEquipment(exercise.Equipment);
            var difficulty = ExerciseMapper.DefaultDifficulty(exercise.Name, exercise.Equipment);
            var instructionText = exercise.Instructions.GetValueOrDefault("en", string.Empty);

            sb.AppendLine($"        Create(\"{guid}\", \"{Escape(exercise.Name)}\", MuscleGroup.{muscleGroup}, Equipment.{equipment}, Difficulty.{difficulty},");
            sb.AppendLine($"            \"{Escape(media.ImageUrl)}\", \"{Escape(media.GifUrl)}\",");
            sb.AppendLine($"            \"{Escape(instructionText)}\"),");
        }

        sb.AppendLine("    ];");
        sb.AppendLine();
        sb.AppendLine("    private static Exercise Create(string id, string name, MuscleGroup muscleGroup, Equipment equipment, Difficulty difficulty, string imageUrl, string gifUrl, string instructionText) => new()");
        sb.AppendLine("    {");
        sb.AppendLine("        Id = Guid.Parse(id),");
        sb.AppendLine("        Name = name,");
        sb.AppendLine("        MuscleGroup = muscleGroup,");
        sb.AppendLine("        Equipment = equipment,");
        sb.AppendLine("        Difficulty = difficulty,");
        sb.AppendLine("        ImageUrl = imageUrl,");
        sb.AppendLine("        GifUrl = gifUrl,");
        sb.AppendLine("        InstructionText = instructionText,");
        sb.AppendLine("        CreatedAt = SeededAt,");
        sb.AppendLine("        UpdatedAt = SeededAt,");
        sb.AppendLine("        IntensityFactor = DefaultIntensityFactor(name)");
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    // Heavy compound lifts score at 1.2x, small-muscle isolation/accessory work at 0.8x,");
        sb.AppendLine("    // everything else (bodyweight compounds, cardio, core) at the 1.0x default.");
        sb.AppendLine("    private static decimal DefaultIntensityFactor(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        string[] isolationKeywords = [\"curl\", \"raise\", \"fly\", \"pushdown\", \"kickback\", \"extension\", \"crunch\"];");
        sb.AppendLine("        if (isolationKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)))");
        sb.AppendLine("        {");
        sb.AppendLine("            return 0.8m;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        string[] compoundKeywords = [\"squat\", \"deadlift\", \"bench press\", \"overhead press\", \"row\", \"pull-up\", \"pull up\", \"clean\", \"snatch\", \"press\"];");
        sb.AppendLine("        if (compoundKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)))");
        sb.AppendLine("        {");
        sb.AppendLine("            return 1.2m;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return 1.0m;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString());
        Console.WriteLine($"Wrote {exercises.Count} exercises to {outputPath}.");
        return 0;
    }

    private static string ToGuid(string datasetId) => $"20000000-0000-0000-0000-{datasetId.PadLeft(12, '0')}";

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static List<DatasetExercise> LoadDataset(string datasetRoot)
    {
        var path = Path.Combine(datasetRoot, "data", "exercises.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<DatasetExercise>>(json) ?? [];
    }

    private static Dictionary<string, MediaMapEntry> LoadMediaMap(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, MediaMapEntry>>(File.ReadAllText(path)) ?? []
            : [];

    private static void SaveMediaMap(string path, Dictionary<string, MediaMapEntry> map) =>
        File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true }));
}
