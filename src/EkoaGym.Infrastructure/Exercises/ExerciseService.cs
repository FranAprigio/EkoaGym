using EkoaGym.Application.Common;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Exercises;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Exercises;

public class ExerciseService(EkoaGymDbContext dbContext) : IExerciseService
{
    public async Task<PagedResult<ExerciseSummaryResponse>> GetExercisesAsync(ExerciseFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Exercises.AsQueryable();

        if (filter.MuscleGroup is not null)
        {
            query = query.Where(e => e.MuscleGroup == filter.MuscleGroup);
        }

        if (filter.Equipment is not null)
        {
            query = query.Where(e => e.Equipment == filter.Equipment);
        }

        if (filter.Difficulty is not null)
        {
            query = query.Where(e => e.Difficulty == filter.Difficulty);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            query = query.Where(e => e.Name.Contains(filter.SearchText));
        }

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => ToSummary(e))
            .ToListAsync(cancellationToken);

        return new PagedResult<ExerciseSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<ExerciseDetailResponse> GetExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exercise = await dbContext.Exercises.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (exercise is null)
        {
            throw new ExerciseNotFoundException();
        }

        return ToDetail(exercise);
    }

    private static ExerciseSummaryResponse ToSummary(Exercise exercise) => new(
        exercise.Id,
        exercise.Name,
        exercise.MuscleGroup,
        exercise.Equipment,
        exercise.Difficulty,
        exercise.ImageUrl,
        exercise.GifUrl);

    private static ExerciseDetailResponse ToDetail(Exercise exercise) => new(
        exercise.Id,
        exercise.Name,
        exercise.MuscleGroup,
        exercise.Equipment,
        exercise.Difficulty,
        exercise.ImageUrl,
        exercise.GifUrl,
        exercise.InstructionText,
        exercise.CreatedAt,
        exercise.UpdatedAt);
}
