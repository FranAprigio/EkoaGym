using EkoaGym.Api.Common;
using EkoaGym.Application.Common;
using EkoaGym.Application.Scoring;
using EkoaGym.Application.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize]
public class SessionsController(IWorkoutSessionService sessionService, IScoringService scoringService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<SessionSummaryResponse>>> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new SessionFilter(from, to, page, pageSize);
        var result = await sessionService.GetSessionsAsync(User.GetUserId(), filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var session = await sessionService.GetSessionAsync(User.GetUserId(), id, cancellationToken);
        return Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<StartSessionResponse>> Start(StartSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await sessionService.StartSessionAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = session.SessionId }, session);
    }

    [HttpPost("{id:guid}/sets")]
    public async Task<ActionResult<LogSetResponse>> LogSet(Guid id, LogSetRequest request, CancellationToken cancellationToken)
    {
        var log = await sessionService.LogSetAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(log);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<CompleteSessionResponse>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sessionService.CompleteSessionAsync(User.GetUserId(), id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/score")]
    public async Task<ActionResult<SessionScoreResponse>> GetScore(Guid id, CancellationToken cancellationToken)
    {
        var result = await scoringService.GetOrComputeScoreAsync(User.GetUserId(), id, cancellationToken);
        return Ok(result);
    }
}
