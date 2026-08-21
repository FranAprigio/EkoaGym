using EkoaGym.Domain.Enums;

namespace EkoaGym.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Sex Sex { get; set; }
    public int HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
