using EkoaGym.Api.Common;
using EkoaGym.Application.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

public class UploadVerificationRequest
{
    public Guid SessionSetLogId { get; set; }
    public IFormFile File { get; set; } = null!;
}

[ApiController]
[Route("api/verification")]
[Authorize]
public class VerificationController(IVerificationService verificationService) : ControllerBase
{
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<VerificationResponse>> Submit([FromForm] UploadVerificationRequest request, CancellationToken cancellationToken)
    {
        await using var stream = request.File.OpenReadStream();
        var upload = new VerificationUpload(request.SessionSetLogId, stream, request.File.ContentType, request.File.Length);
        var response = await verificationService.SubmitAsync(User.GetUserId(), upload, cancellationToken);
        return Ok(response);
    }
}
