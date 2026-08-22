using System.Text.Json;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Leagues;
using EkoaGym.Application.Scoring;
using EkoaGym.Domain.Entities;
using EkoaGym.Domain.Enums;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Scoring;

public class ScoringService(EkoaGymDbContext dbContext, ILeagueService leagueService) : IScoringService
{
    private const decimal ConsistencyBonusAmount = 50m;
    private const decimal PrBonusAmount = 100m;
    private const int ConsistencySessionsRequired = 3;
    private static readonly TimeSpan ConsistencyWindow = TimeSpan.FromDays(7);
    private static readonly TimeSpan PrLookbackWindow = TimeSpan.FromDays(90);

    public async Task<SessionScoreResponse> GetOrComputeScoreAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await dbContext.WorkoutSessions.SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            throw new SessionNotFoundException();
        }

        if (session.EndedAt is null)
        {
            throw new SessionNotCompletedException();
        }

        var existingScore = await dbContext.SessionScores.SingleOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        if (existingScore is not null)
        {
            return ToResponse(existingScore);
        }

        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null)
        {
            throw new ProfileNotFoundException();
        }

        var sets = await dbContext.SessionSetLogs
            .Where(l => l.SessionId == sessionId)
            .Select(l => new
            {
                l.ExerciseId,
                ExerciseName = l.Exercise.Name,
                l.Exercise.IntensityFactor,
                l.ActualReps,
                l.ActualWeightKg,
                l.IsSuspicious,
                l.VerificationStatus
            })
            .ToListAsync(cancellationToken);

        var breakdown = new List<ExerciseScoreBreakdown>();
        var totalScore = 0m;
        var hasPr = false;
        var prLookbackCutoff = session.StartedAt.Add(-PrLookbackWindow);
        var consistencyWindowStart = session.StartedAt.Add(-ConsistencyWindow);

        foreach (var group in sets.GroupBy(s => new { s.ExerciseId, s.ExerciseName }))
        {
            var includedSets = group.Where(s => !(s.IsSuspicious && s.VerificationStatus != VerificationStatus.Approved)).ToList();
            var exerciseScore = includedSets.Sum(s => s.ActualReps * s.ActualWeightKg / profile.CurrentWeightKg * s.IntensityFactor);
            var hasPendingSets = group.Any(s => s.IsSuspicious && s.VerificationStatus != VerificationStatus.Approved);

            breakdown.Add(new ExerciseScoreBreakdown(group.Key.ExerciseId, group.Key.ExerciseName, Math.Round(exerciseScore, 2), hasPendingSets));
            totalScore += exerciseScore;

            var priorMaxWeight = await dbContext.SessionSetLogs
                .Where(l => l.ExerciseId == group.Key.ExerciseId
                    && l.Session.UserId == userId
                    && l.Session.Id != sessionId
                    && l.Session.StartedAt >= prLookbackCutoff
                    && l.Session.StartedAt < session.StartedAt)
                .Select(l => (decimal?)l.ActualWeightKg)
                .MaxAsync(cancellationToken) ?? 0m;

            if (includedSets.Any(s => s.ActualWeightKg > priorMaxWeight))
            {
                hasPr = true;
            }
        }

        var recentCompletedSessionCount = await dbContext.WorkoutSessions
            .CountAsync(s => s.UserId == userId && s.EndedAt != null && s.StartedAt >= consistencyWindowStart && s.StartedAt <= session.StartedAt, cancellationToken);

        var consistencyBonus = recentCompletedSessionCount >= ConsistencySessionsRequired ? ConsistencyBonusAmount : 0m;
        var prBonus = hasPr ? PrBonusAmount : 0m;
        totalScore = Math.Round(totalScore + consistencyBonus + prBonus, 2);

        var score = new SessionScore
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            TotalScore = totalScore,
            ConsistencyBonus = consistencyBonus,
            PrBonus = prBonus,
            BreakdownJson = JsonSerializer.Serialize(breakdown),
            ComputedAt = DateTime.UtcNow
        };
        dbContext.SessionScores.Add(score);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Only reached once per session, since a second call returns the persisted score above — points
        // are never double-credited to the user's league membership.
        await leagueService.AddPointsAsync(userId, totalScore, cancellationToken);

        return new SessionScoreResponse(sessionId, totalScore, consistencyBonus, prBonus, breakdown);
    }

    private static SessionScoreResponse ToResponse(SessionScore score)
    {
        var breakdown = JsonSerializer.Deserialize<List<ExerciseScoreBreakdown>>(score.BreakdownJson) ?? [];
        return new SessionScoreResponse(score.SessionId, score.TotalScore, score.ConsistencyBonus, score.PrBonus, breakdown);
    }
}
