using EkoaGym.Domain.Enums;

namespace EkoaGym.Domain.Entities;

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
    public Difficulty Difficulty { get; set; }
    public string? ImageUrl { get; set; }
    // 180x180 looping animation (Cloudinary-hosted) demonstrating the exercise; distinct from the static ImageUrl thumbnail.
    public string? GifUrl { get; set; }
    public string InstructionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // M-10.2: relative-tonnage scoring multiplier (1.2 compound / 1.0 default / 0.8 isolation) — data
    // driven from here, never hardcoded client-side.
    public decimal IntensityFactor { get; set; } = 1.0m;
}
