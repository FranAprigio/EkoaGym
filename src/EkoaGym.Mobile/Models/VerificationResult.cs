namespace EkoaGym.Mobile.Models;

public record VerificationResult(Guid Id, Guid SessionSetLogId, string Status, DateTime SubmittedAt);
