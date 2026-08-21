using EkoaGym.Api.Common;
using EkoaGym.Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController(IProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetProfileAsync(User.GetUserId(), cancellationToken);
        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Put(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await profileService.UpdateProfileAsync(User.GetUserId(), request, cancellationToken);
        return Ok(profile);
    }
}
