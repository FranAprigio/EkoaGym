namespace EkoaGym.Application.ProgressPhotos;

// Decouples the Application layer from ASP.NET Core's IFormFile.
public record ProgressPhotoUpload(DateOnly Date, Stream Content, string ContentType, long Length);
