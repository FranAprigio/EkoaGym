namespace EkoaGym.Domain.Entities;

public class ProgressPhoto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateOnly Date { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    // Not in the original tech note; needed to serve the correct Content-Type/extension via the authenticated streaming endpoint.
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
