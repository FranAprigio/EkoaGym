namespace EkoaGym.Application.ProgressPhotos;

public interface IProgressPhotoService
{
    Task<ProgressPhotoResponse> CreateProgressPhotoAsync(Guid userId, ProgressPhotoUpload upload, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProgressPhotoResponse>> GetProgressPhotosAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProgressPhotoContent> GetProgressPhotoContentAsync(Guid userId, Guid photoId, CancellationToken cancellationToken = default);
}
