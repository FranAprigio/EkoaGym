using EkoaGym.Domain.Enums;

namespace EkoaGym.Domain.Entities;

// Proof submitted for a SessionSetLog flagged as suspicious (M-10.4). Resolution (Approved/Rejected)
// is a manual moderation action with no admin endpoint in this epic — rows stay PendingReview until
// a future admin/moderation epic adds one.
public class VerificationRequest
{
    public Guid Id { get; set; }
    public Guid SessionSetLogId { get; set; }
    public SessionSetLog SessionSetLog { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public VerificationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}
