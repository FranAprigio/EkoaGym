using EkoaGym.Api.Common;
using EkoaGym.Application.Plans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/plans")]
[Authorize]
public class PlansController(IWorkoutPlanService planService, IProgressionService progressionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanSummaryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var plans = await planService.GetPlansAsync(User.GetUserId(), cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var plan = await planService.GetPlanAsync(User.GetUserId(), id, cancellationToken);
        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanDetailResponse>> Create(CreatePlanRequest request, CancellationToken cancellationToken)
    {
        var plan = await planService.CreatePlanAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PlanDetailResponse>> Update(Guid id, UpdatePlanRequest request, CancellationToken cancellationToken)
    {
        var plan = await planService.UpdatePlanAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(plan);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await planService.DeletePlanAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<ActionResult<PlanDetailResponse>> Duplicate(Guid id, CancellationToken cancellationToken)
    {
        var plan = await planService.DuplicatePlanAsync(User.GetUserId(), id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
    }

    [HttpGet("{planId:guid}/exercises/{exerciseId:guid}/next-target")]
    public async Task<ActionResult<NextTargetResponse>> GetNextTarget(Guid planId, Guid exerciseId, CancellationToken cancellationToken)
    {
        var nextTarget = await progressionService.GetNextTargetAsync(User.GetUserId(), planId, exerciseId, cancellationToken);
        return Ok(nextTarget);
    }
}
