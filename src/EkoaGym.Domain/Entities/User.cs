namespace EkoaGym.Domain.Entities;

// Placeholder entity to unblock the initial EF Core migration; real user modeling comes with the auth epic.
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
