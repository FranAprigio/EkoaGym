using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Verification;

// Application-level abstraction over the uploaded proof file — mirrors ProgressPhotoUpload's pattern
// so the Api layer adapts IFormFile -> this record the same way.
public record VerificationUpload(Guid SessionSetLogId, Stream Content, string ContentType, long Length);

public record VerificationResponse(Guid Id, Guid SessionSetLogId, VerificationStatus Status, DateTime SubmittedAt);
