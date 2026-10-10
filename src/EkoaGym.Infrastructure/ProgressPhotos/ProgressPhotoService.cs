using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.ProgressPhotos;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.ProgressPhotos;

public class ProgressPhotoService(EkoaGymDbContext dbContext, IOptions<FileStorageOptions> fileStorageOptions) : IProgressPhotoService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private readonly FileStorageOptions _options = fileStorageOptions.Value;

    public async Task<ProgressPhotoResponse> CreateProgressPhotoAsync(Guid userId, ProgressPhotoUpload upload, CancellationToken cancellationToken = default)
    {
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
            throw new InvalidFileUploadException("image_type_not_allowed", "Only JPEG, PNG, or WEBP images are allowed.");
        }

        var photo = new ProgressPhoto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = upload.Date,
            ContentType = upload.ContentType,
            CreatedAt = DateTime.UtcNow
        };
        // ImageUrl is a synthetic, authenticated API route (not a public static path) so only the owner can fetch it.
        photo.ImageUrl = $"/api/progress-photos/{photo.Id}/image";

        var directory = GetUserPhotosDirectory(userId);
        Directory.CreateDirectory(directory);

        // Filename is derived solely from the photo's own GUID + a whitelisted extension, never from client input.
        var physicalPath = Path.Combine(directory, $"{photo.Id}{extension}");
        await using (var fileStream = File.Create(physicalPath))
        {
            await upload.Content.CopyToAsync(fileStream, cancellationToken);
        }

        dbContext.ProgressPhotos.Add(photo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(photo);
    }

    public async Task<IReadOnlyList<ProgressPhotoResponse>> GetProgressPhotosAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProgressPhotos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Date)
            .Select(p => ToResponse(p))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProgressPhotoContent> GetProgressPhotoContentAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default)
    {
        var photo = await dbContext.ProgressPhotos.SingleOrDefaultAsync(p => p.Id == photoId, cancellationToken);
        if (photo is null || photo.UserId != userId)
        {
            // 404 (not 403) for another user's photo, to avoid leaking existence.
            throw new ProgressPhotoNotFoundException();
        }

        var extension = AllowedContentTypes.GetValueOrDefault(photo.ContentType, ".bin");
        var physicalPath = Path.Combine(GetUserPhotosDirectory(userId), $"{photo.Id}{extension}");
        if (!File.Exists(physicalPath))
        {
            throw new ProgressPhotoNotFoundException();
        }

        var stream = File.OpenRead(physicalPath);
        return new ProgressPhotoContent(stream, photo.ContentType);
    }

    private string GetUserPhotosDirectory(Guid userId) => Path.Combine(_options.UploadsRootPath, "progress-photos", userId.ToString());

    private static ProgressPhotoResponse ToResponse(ProgressPhoto photo) => new(photo.Id, photo.Date, photo.ImageUrl, photo.CreatedAt);
}
