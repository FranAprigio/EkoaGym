namespace EkoaGym.Application.ProgressPhotos;

public record ProgressPhotoResponse(Guid Id, DateOnly Date, string ImageUrl, DateTime CreatedAt);
