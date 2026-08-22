using EkoaGym.Application.Common;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Feed;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Feed;

public class FeedService(EkoaGymDbContext dbContext) : IFeedService
{
    public async Task<PagedResult<FeedCardResponse>> GetFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = dbContext.WorkoutSessions.Where(s => s.EndedAt != null);
        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .OrderByDescending(s => s.EndedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(dbContext.Users, s => s.UserId, u => u.Id, (s, u) => new { s.Id, s.UserId, UserName = u.Name, s.StartedAt, EndedAt = s.EndedAt!.Value })
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();

        var setLogs = await dbContext.SessionSetLogs
            .Where(l => sessionIds.Contains(l.SessionId))
            .Select(l => new { l.SessionId, l.ExerciseId, l.Exercise.MuscleGroup, l.ActualReps, l.ActualWeightKg })
            .ToListAsync(cancellationToken);

        var reactionCounts = await dbContext.FeedReactions
            .Where(r => sessionIds.Contains(r.SessionId))
            .GroupBy(r => r.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var reactedByMe = await dbContext.FeedReactions
            .Where(r => sessionIds.Contains(r.SessionId) && r.UserId == userId)
            .Select(r => r.SessionId)
            .ToListAsync(cancellationToken);

        var items = new List<FeedCardResponse>();
        foreach (var session in sessions)
        {
            var logsForSession = setLogs.Where(l => l.SessionId == session.Id).ToList();
            var totalTonnage = logsForSession.Sum(l => l.ActualReps * l.ActualWeightKg);
            var muscleGroupVolumes = logsForSession
                .GroupBy(l => l.MuscleGroup)
                .Select(g => new MuscleGroupVolume(g.Key, g.Sum(x => x.ActualReps * x.ActualWeightKg)))
                .ToList();

            var hasPr = false;
            foreach (var exerciseGroup in logsForSession.GroupBy(l => l.ExerciseId))
            {
                var bestWeightThisSession = exerciseGroup.Max(l => l.ActualWeightKg);
                var priorMax = await dbContext.SessionSetLogs
                    .Where(l => l.ExerciseId == exerciseGroup.Key && l.Session.UserId == session.UserId && l.Session.StartedAt < session.StartedAt)
                    .Select(l => (decimal?)l.ActualWeightKg)
                    .MaxAsync(cancellationToken) ?? 0m;

                if (bestWeightThisSession > priorMax)
                {
                    hasPr = true;
                    break;
                }
            }

            var reactionCount = reactionCounts.FirstOrDefault(r => r.SessionId == session.Id)?.Count ?? 0;

            items.Add(new FeedCardResponse(
                session.Id,
                session.UserId,
                session.UserName,
                session.StartedAt,
                session.EndedAt,
                totalTonnage,
                (int)(session.EndedAt - session.StartedAt).TotalMinutes,
                hasPr,
                muscleGroupVolumes,
                reactionCount,
                reactedByMe.Contains(session.Id)));
        }

        return new PagedResult<FeedCardResponse>(items, page, pageSize, totalCount);
    }

    public async Task<FeedReactionResponse> ToggleReactionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var sessionExists = await dbContext.WorkoutSessions.AnyAsync(s => s.Id == sessionId && s.EndedAt != null, cancellationToken);
        if (!sessionExists)
        {
            throw new SessionNotFoundException();
        }

        var existing = await dbContext.FeedReactions.SingleOrDefaultAsync(r => r.SessionId == sessionId && r.UserId == userId, cancellationToken);
        bool reacted;
        if (existing is not null)
        {
            dbContext.FeedReactions.Remove(existing);
            reacted = false;
        }
        else
        {
            dbContext.FeedReactions.Add(new FeedReaction { Id = Guid.NewGuid(), SessionId = sessionId, UserId = userId, CreatedAt = DateTime.UtcNow });
            reacted = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var count = await dbContext.FeedReactions.CountAsync(r => r.SessionId == sessionId, cancellationToken);
        return new FeedReactionResponse(sessionId, reacted, count);
    }
}
