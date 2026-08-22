using EkoaGym.Api.Common;
using EkoaGym.Application.Common;
using EkoaGym.Application.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController(IFeedService feedService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FeedCardResponse>>> GetFeed(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await feedService.GetFeedAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/react")]
    public async Task<ActionResult<FeedReactionResponse>> React(Guid id, CancellationToken cancellationToken)
    {
        var result = await feedService.ToggleReactionAsync(User.GetUserId(), id, cancellationToken);
        return Ok(result);
    }
}
