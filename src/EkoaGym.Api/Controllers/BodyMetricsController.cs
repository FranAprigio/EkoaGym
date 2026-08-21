using EkoaGym.Api.Common;
using EkoaGym.Application.BodyMetrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/body-metrics")]
[Authorize]
public class BodyMetricsController(IBodyMetricService bodyMetricService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BodyMetricResponse>>> GetAll(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var filter = new BodyMetricFilter(from, to);
        var logs = await bodyMetricService.GetBodyMetricsAsync(User.GetUserId(), filter, cancellationToken);
        return Ok(logs);
    }

    [HttpPost]
    public async Task<ActionResult<BodyMetricResponse>> Create(CreateBodyMetricRequest request, CancellationToken cancellationToken)
    {
        var log = await bodyMetricService.CreateBodyMetricAsync(User.GetUserId(), request, cancellationToken);
        return Ok(log);
    }
}
