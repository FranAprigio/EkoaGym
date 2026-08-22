using EkoaGym.Application.Common;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Plans;
using EkoaGym.Application.Sessions;
using EkoaGym.Domain.Entities;
using EkoaGym.Domain.Enums;
using EkoaGym.Infrastructure.ExerciseLimits;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Sessions;

public class WorkoutSessionService(EkoaGymDbContext dbContext) : IWorkoutSessionService
{
    // M-10.4: a set logged more than 30% above the user's own 90-day PR for that exercise gets
    // suspended for league scoring and needs photo/video proof — the workout log itself is never lost.
    private const decimal SuspiciousThresholdMultiplier = 1.3m;
    private static readonly TimeSpan PrLookbackWindow = TimeSpan.FromDays(90);
    public async Task<StartSessionResponse> StartSessionAsync(Guid userId, StartSessionRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.WorkoutPlans.SingleOrDefaultAsync(p => p.Id == request.PlanId && !p.IsDeleted, cancellationToken);
        if (plan is null)
        {
            throw new PlanNotFoundException();
        }

        if (plan.UserId != userId)
        {
            throw new PlanAccessDeniedException();
        }

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.WorkoutSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        var exercises = await dbContext.WorkoutPlanExercises
            .Where(pe => pe.PlanId == plan.Id)
            .OrderBy(pe => pe.OrderIndex)
            .Join(dbContext.Exercises, pe => pe.ExerciseId, e => e.Id, (pe, e) => new PlanExerciseResponse(
                pe.Id, pe.ExerciseId, e.Name, pe.Sets, pe.Reps, pe.WeightKg, pe.RestSeconds, pe.OrderIndex))
            .ToListAsync(cancellationToken);

        return new StartSessionResponse(session.Id, plan.Id, session.StartedAt, exercises);
    }

    public async Task<LogSetResponse> LogSetAsync(Guid userId, Guid sessionId, LogSetRequest request, CancellationToken cancellationToken = default)
    {
        var session = await FindOwnedSessionAsync(userId, sessionId, cancellationToken);
        if (session.EndedAt is not null)
        {
            throw new SessionAlreadyCompletedException();
        }

        var isPlannedExercise = await dbContext.WorkoutPlanExercises
            .AnyAsync(pe => pe.PlanId == session.PlanId && pe.ExerciseId == request.ExerciseId, cancellationToken);
        if (!isPlannedExercise)
        {
            throw new PlanExerciseNotFoundException();
        }

        var exercise = await dbContext.Exercises.SingleAsync(e => e.Id == request.ExerciseId, cancellationToken);

        // M-10.3: the client-side cap is a UX convenience only — this is the real security boundary.
        var explicitCap = await dbContext.ExerciseLimits
            .Where(l => l.ExerciseId == request.ExerciseId)
            .Select(l => (decimal?)l.MaxWeightKg)
            .SingleOrDefaultAsync(cancellationToken);
        var maxWeightKg = explicitCap ?? ExerciseLimitDefaults.GetDefaultCap(exercise.MuscleGroup);
        if (request.ActualWeightKg > maxWeightKg)
        {
            throw new WeightLimitExceededException(maxWeightKg);
        }

        // EF Core can't translate DateTime.Add() inside a query expression — compute the cutoff first.
        var prLookbackCutoff = DateTime.UtcNow.Add(-PrLookbackWindow);
        var priorMaxWeight = await dbContext.SessionSetLogs
            .Where(l => l.ExerciseId == request.ExerciseId
                && l.Session.UserId == userId
                && l.SessionId != sessionId
                && l.Session.StartedAt >= prLookbackCutoff)
            .Select(l => (decimal?)l.ActualWeightKg)
            .MaxAsync(cancellationToken) ?? 0m;
        var isSuspicious = priorMaxWeight > 0 && request.ActualWeightKg > priorMaxWeight * SuspiciousThresholdMultiplier;
        var log = new SessionSetLog
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ExerciseId = request.ExerciseId,
            SetNumber = request.SetNumber,
            ActualReps = request.ActualReps,
            ActualWeightKg = request.ActualWeightKg,
            CompletedAt = DateTime.UtcNow,
            IsSuspicious = isSuspicious,
            VerificationStatus = isSuspicious ? VerificationStatus.PendingReview : VerificationStatus.None
        };

        dbContext.SessionSetLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LogSetResponse(log.Id, log.ExerciseId, log.SetNumber, log.ActualReps, log.ActualWeightKg, log.CompletedAt, log.IsSuspicious, log.VerificationStatus);
    }

    public async Task<CompleteSessionResponse> CompleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await FindOwnedSessionAsync(userId, sessionId, cancellationToken);

        if (session.EndedAt is null)
        {
            session.EndedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new CompleteSessionResponse(session.Id, session.StartedAt, session.EndedAt!.Value);
    }

    public async Task<PagedResult<SessionSummaryResponse>> GetSessionsAsync(Guid userId, SessionFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.WorkoutSessions.Where(s => s.UserId == userId);

        if (filter.From is not null)
        {
            query = query.Where(s => s.StartedAt >= filter.From.Value);
        }

        if (filter.To is not null)
        {
            query = query.Where(s => s.StartedAt <= filter.To.Value);
        }

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);

        var sessionIds = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var sessions = await dbContext.WorkoutSessions
            .Where(s => sessionIds.Contains(s.Id))
            .Join(dbContext.WorkoutPlans, s => s.PlanId, p => p.Id, (s, p) => new { s.Id, s.PlanId, PlanName = p.Name, s.StartedAt, s.EndedAt })
            .ToListAsync(cancellationToken);

        var logAggregates = await dbContext.SessionSetLogs
            .Where(l => sessionIds.Contains(l.SessionId))
            .GroupBy(l => l.SessionId)
            .Select(g => new
            {
                SessionId = g.Key,
                ExerciseCount = g.Select(x => x.ExerciseId).Distinct().Count(),
                TotalVolumeKg = g.Sum(x => x.ActualReps * x.ActualWeightKg)
            })
            .ToListAsync(cancellationToken);

        var items = sessions
            .OrderByDescending(s => s.StartedAt)
            .Select(s =>
            {
                var aggregate = logAggregates.SingleOrDefault(a => a.SessionId == s.Id);
                return new SessionSummaryResponse(
                    s.Id,
                    s.PlanId,
                    s.PlanName,
                    s.StartedAt,
                    s.EndedAt,
                    s.EndedAt.HasValue ? s.EndedAt.Value - s.StartedAt : null,
                    aggregate?.ExerciseCount ?? 0,
                    aggregate?.TotalVolumeKg ?? 0m);
            })
            .ToList();

        return new PagedResult<SessionSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<SessionDetailResponse> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await FindOwnedSessionAsync(userId, sessionId, cancellationToken);

        var planName = await dbContext.WorkoutPlans
            .Where(p => p.Id == session.PlanId)
            .Select(p => p.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var sets = await dbContext.SessionSetLogs
            .Where(l => l.SessionId == session.Id)
            .OrderBy(l => l.CompletedAt)
            .Join(dbContext.Exercises, l => l.ExerciseId, e => e.Id, (l, e) => new SessionSetLogResponse(
                l.Id, l.ExerciseId, e.Name, l.SetNumber, l.ActualReps, l.ActualWeightKg, l.CompletedAt))
            .ToListAsync(cancellationToken);

        return new SessionDetailResponse(session.Id, session.PlanId, planName, session.StartedAt, session.EndedAt, sets);
    }

    private async Task<WorkoutSession> FindOwnedSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.WorkoutSessions.SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
        {
            // 404 (not 403) for another user's session, to avoid leaking existence.
            throw new SessionNotFoundException();
        }

        return session;
    }
}
