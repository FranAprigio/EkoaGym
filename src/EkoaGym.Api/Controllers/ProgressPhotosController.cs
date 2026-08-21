using EkoaGym.Api.Common;
using EkoaGym.Application.ProgressPhotos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

// Multipart form fields for the progress photo upload endpoint (kept in Api since IFormFile is ASP.NET Core-specific).
public class UploadProgressPhotoRequest
{
    public IFormFile File { get; set; } = null!;
    public DateOnly Date { get; set; }
}

[ApiController]
[Route("api/progress-photos")]
[Authorize]
public class ProgressPhotosController(IProgressPhotoService progressPhotoService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgressPhotoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var photos = await progressPhotoService.GetProgressPhotosAsync(User.GetUserId(), cancellationToken);
        return Ok(photos);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ProgressPhotoResponse>> Upload([FromForm] UploadProgressPhotoRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var upload = new ProgressPhotoUpload(request.Date, stream, request.File.ContentType, request.File.Length);
        var photo = await progressPhotoService.CreateProgressPhotoAsync(User.GetUserId(), upload, cancellationToken);
        return Ok(photo);
    }

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var content = await progressPhotoService.GetProgressPhotoContentAsync(User.GetUserId(), id, cancellationToken);
        return File(content.Content, content.ContentType);
    }
}
