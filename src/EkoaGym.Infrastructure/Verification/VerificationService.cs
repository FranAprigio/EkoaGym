using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Verification;
using EkoaGym.Domain.Entities;
using EkoaGym.Domain.Enums;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.Verification;

public class VerificationService(EkoaGymDbContext dbContext, IOptions<FileStorageOptions> fileStorageOptions) : IVerificationService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["video/mp4"] = ".mp4"
    };

    private readonly FileStorageOptions _options = fileStorageOptions.Value;

    public async Task<VerificationResponse> SubmitAsync(Guid userId, VerificationUpload upload, CancellationToken cancellationToken = default)
    {
        var setLog = await dbContext.SessionSetLogs
            .Include(l => l.Session)
            .SingleOrDefaultAsync(l => l.Id == upload.SessionSetLogId, cancellationToken);
        if (setLog is null || setLog.Session.UserId != userId)
        {
            throw new SessionSetLogNotFoundException();
        }

        if (upload.Length <= 0)
        {
            throw new InvalidFileUploadException("file_empty", "The uploaded file is empty.");
        }

        if (upload.Length > MaxFileSizeBytes)
        {
            throw new InvalidFileUploadException(
                "file_too_large",
                $"The uploaded file exceeds the {MaxFileSizeBytes / (1024 * 1024)}MB limit.",
                new Dictionary<string, object?> { ["maxMb"] = MaxFileSizeBytes / (1024 * 1024) });
        }

        if (!AllowedContentTypes.TryGetValue(upload.ContentType, out var extension))
        {
            throw new InvalidFileUploadException("proof_type_not_allowed", "Only JPEG, PNG, WEBP images or MP4 video are allowed.");
        }

        var verification = await dbContext.VerificationRequests
            .SingleOrDefaultAsync(v => v.SessionSetLogId == upload.SessionSetLogId, cancellationToken);
        var isNew = verification is null;
        verification ??= new VerificationRequest { Id = Guid.NewGuid(), SessionSetLogId = upload.SessionSetLogId, UserId = userId };

        var directory = Path.Combine(_options.UploadsRootPath, "verification", userId.ToString());
        Directory.CreateDirectory(directory);

        // Filename is derived from the request's own GUID + a whitelisted extension, never client input.
        var physicalPath = Path.Combine(directory, $"{verification.Id}{extension}");
        await using (var fileStream = File.Create(physicalPath))
        {
            await upload.Content.CopyToAsync(fileStream, cancellationToken);
        }

        verification.ContentType = upload.ContentType;
        // Not served back to clients (no streaming endpoint exists for this yet) — a reference for a
        // future moderator/admin tool that resolves PendingReview requests.
        verification.ImageUrl = physicalPath;
        verification.Status = VerificationStatus.PendingReview;
        verification.SubmittedAt = DateTime.UtcNow;

        setLog.VerificationStatus = VerificationStatus.PendingReview;

        if (isNew)
        {
            dbContext.VerificationRequests.Add(verification);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new VerificationResponse(verification.Id, verification.SessionSetLogId, verification.Status, verification.SubmittedAt);
    }
}
