using EkoaGym.Application.Common;
using EkoaGym.Application.Exercises;
using EkoaGym.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/exercises")]
[AllowAnonymous] // Shared reference data; intentionally public, no JWT required.
public class ExercisesController(IExerciseService exerciseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ExerciseSummaryResponse>>> GetAll(
        [FromQuery] MuscleGroup? muscleGroup,
        [FromQuery] Equipment? equipment,
        [FromQuery] Difficulty? difficulty,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new ExerciseFilter(muscleGroup, equipment, difficulty, q, page, pageSize);
        var result = await exerciseService.GetExercisesAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciseDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var exercise = await exerciseService.GetExerciseByIdAsync(id, cancellationToken);
        return Ok(exercise);
    }
}
