using EkoaGym.Api.Common;
using EkoaGym.Application.Progress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

// Separate from ExercisesController: that controller is [AllowAnonymous], and AllowAnonymous on a controller
// cannot be overridden by [Authorize] on one of its actions (it short-circuits auth for the whole endpoint).
[ApiController]
[Route("api/exercises/{exerciseId:guid}/progress")]
[Authorize]
public class ExerciseProgressController(IExerciseProgressService exerciseProgressService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExerciseProgressPoint>>> Get(
        Guid exerciseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var progress = await exerciseProgressService.GetExerciseProgressAsync(User.GetUserId(), exerciseId, from, to, cancellationToken);
        return Ok(progress);
    }
}
