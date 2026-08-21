using EkoaGym.Api.Common;
using EkoaGym.Application.Goals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/goals")]
[Authorize]
public class GoalsController(IGoalService goalService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GoalResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var goals = await goalService.GetGoalsAsync(User.GetUserId(), cancellationToken);
        return Ok(goals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var goal = await goalService.GetGoalAsync(User.GetUserId(), id, cancellationToken);
        return Ok(goal);
    }

    [HttpPost]
    public async Task<ActionResult<GoalResponse>> Create(CreateGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await goalService.CreateGoalAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GoalResponse>> Update(Guid id, UpdateGoalRequest request, CancellationToken cancellationToken)
    {
        var goal = await goalService.UpdateGoalAsync(User.GetUserId(), id, request, cancellationToken);
        return Ok(goal);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await goalService.DeleteGoalAsync(User.GetUserId(), id, cancellationToken);
        return NoContent();
    }
}
