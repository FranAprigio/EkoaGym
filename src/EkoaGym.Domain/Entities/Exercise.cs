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
    public string InstructionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
