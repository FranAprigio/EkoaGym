using EkoaGym.Api.Common;
using EkoaGym.Application.Leagues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/leagues")]
[Authorize]
public class LeaguesController(ILeagueService leagueService) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<LeagueCurrentResponse>> GetCurrent([FromQuery] int tzOffsetMinutes = 0, CancellationToken cancellationToken = default)
    {
        var result = await leagueService.GetCurrentAsync(User.GetUserId(), tzOffsetMinutes, cancellationToken);
        return Ok(result);
    }
}
