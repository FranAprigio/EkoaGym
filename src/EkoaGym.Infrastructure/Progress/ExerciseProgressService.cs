using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Progress;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Progress;

public class ExerciseProgressService(EkoaGymDbContext dbContext) : IExerciseProgressService
{
    public async Task<IReadOnlyList<ExerciseProgressPoint>> GetExerciseProgressAsync(
        Guid userId, Guid exerciseId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var exerciseExists = await dbContext.Exercises.AnyAsync(e => e.Id == exerciseId, cancellationToken);
        if (!exerciseExists)
        {
            throw new ExerciseNotFoundException();
        }

        var query = dbContext.SessionSetLogs.Where(l => l.ExerciseId == exerciseId && l.Session.UserId == userId);

        if (from is not null)
        {
            query = query.Where(l => l.Session.StartedAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(l => l.Session.StartedAt <= to.Value);
        }

        var logs = await query
            .OrderBy(l => l.Session.StartedAt)
            .ThenBy(l => l.CompletedAt)
            .Select(l => new { l.SessionId, SessionStartedAt = l.Session.StartedAt, l.ActualReps, l.ActualWeightKg })
            .ToListAsync(cancellationToken);

        var points = new List<ExerciseProgressPoint>();
        var runningMaxWeightEver = 0m;
        var priorSets = new List<(decimal WeightKg, int Reps)>();

        // Chronological, one point per session, with PR flags computed relative to everything before it.
        foreach (var sessionGroup in logs.GroupBy(l => l.SessionId))
        {
            var sets = sessionGroup.ToList();
            var totalSets = sets.Count;
            var totalReps = sets.Sum(s => s.ActualReps);
            var totalVolumeKg = sets.Sum(s => s.ActualWeightKg * s.ActualReps);
            var best = sets
                .OrderByDescending(s => s.ActualWeightKg * s.ActualReps)
                .ThenByDescending(s => s.ActualWeightKg)
                .ThenByDescending(s => s.ActualReps)
                .First();

            var isWeightPR = best.ActualWeightKg > runningMaxWeightEver;
            var maxRepsAtOrAboveWeight = priorSets
                .Where(s => s.WeightKg >= best.ActualWeightKg)
                .Select(s => (int?)s.Reps)
                .Max() ?? 0;
            var isRepPR = best.ActualReps > maxRepsAtOrAboveWeight;

            points.Add(new ExerciseProgressPoint(
                sessionGroup.Key,
                sets[0].SessionStartedAt,
                best.ActualReps,
                best.ActualWeightKg,
                totalVolumeKg,
                totalSets,
                totalReps,
                isWeightPR,
                isRepPR));

            runningMaxWeightEver = Math.Max(runningMaxWeightEver, sets.Max(s => s.ActualWeightKg));
            priorSets.AddRange(sets.Select(s => (s.ActualWeightKg, s.ActualReps)));
        }

        return points;
    }
}
