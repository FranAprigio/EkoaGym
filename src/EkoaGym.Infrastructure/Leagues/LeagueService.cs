using EkoaGym.Application.Leagues;
using EkoaGym.Domain.Entities;
using EkoaGym.Domain.Enums;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Leagues;

public class LeagueService(EkoaGymDbContext dbContext) : ILeagueService
{
    private const int RoomCapacity = 30;
    private const int PromotionZoneSize = 5;
    private const int RelegationZoneSize = 10;

    public async Task<LeagueCurrentResponse> GetCurrentAsync(Guid userId, int tzOffsetMinutes, CancellationToken cancellationToken = default)
    {
        var membership = await EnsureCurrentMembershipAsync(userId, tzOffsetMinutes, cancellationToken);
        var room = await dbContext.LeagueRooms.SingleAsync(r => r.Id == membership.RoomId, cancellationToken);

        var standings = await dbContext.LeagueMemberships
            .Where(m => m.RoomId == room.Id)
            .Join(dbContext.Users, m => m.UserId, u => u.Id, (m, u) => new { m.UserId, u.Name, m.Points })
            .OrderByDescending(m => m.Points)
            .ThenBy(m => m.UserId)
            .ToListAsync(cancellationToken);

        var standingsResponse = standings
            .Select((s, index) => new LeagueMemberResponse(s.UserId, s.Name, index + 1, s.Points, s.UserId == userId))
            .ToList();

        var rank = standingsResponse.First(s => s.IsCurrentUser).Rank;
        var (weekStartUtc, weekEndUtc) = ComputeWeekUtcRange(tzOffsetMinutes);

        return new LeagueCurrentResponse(
            room.Tier,
            room.Id,
            rank,
            membership.Points,
            standingsResponse,
            weekStartUtc,
            weekEndUtc,
            membership.MovementSincePreviousWeek,
            room.Tier == LeagueTier.Titanium ? 0 : PromotionZoneSize,
            room.Tier == LeagueTier.Bronze ? 0 : RelegationZoneSize);
    }

    // Used by ScoringService to credit points to the user's current-week membership, creating one
    // (with correct tier carry-over) if this is their first activity of the week.
    public async Task AddPointsAsync(Guid userId, decimal points, CancellationToken cancellationToken = default)
    {
        // Point-crediting doesn't need to be pixel-perfect on the exact local week boundary the way
        // "which room am I looking at" does — UTC is an acceptable simplification here.
        var membership = await EnsureCurrentMembershipAsync(userId, tzOffsetMinutes: 0, cancellationToken);
        membership.Points += points;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<LeagueMembership> EnsureCurrentMembershipAsync(Guid userId, int tzOffsetMinutes, CancellationToken cancellationToken)
    {
        var weekStart = ComputeLocalWeekStart(tzOffsetMinutes);

        var existing = await dbContext.LeagueMemberships
            .Include(m => m.Room)
            .Where(m => m.UserId == userId && m.Room.WeekStartDate == weekStart)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var (tier, movement) = await DetermineTierAsync(userId, cancellationToken);
        var room = await FindOrCreateOpenRoomAsync(tier, weekStart, cancellationToken);

        var membership = new LeagueMembership
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            UserId = userId,
            Points = 0,
            MovementSincePreviousWeek = movement,
            JoinedAt = DateTime.UtcNow
        };

        dbContext.LeagueMemberships.Add(membership);
        await dbContext.SaveChangesAsync(cancellationToken);

        membership.Room = room;
        return membership;
    }

    private async Task<(LeagueTier Tier, LeagueMovement Movement)> DetermineTierAsync(Guid userId, CancellationToken cancellationToken)
    {
        var previous = await dbContext.LeagueMemberships
            .Include(m => m.Room)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Room.WeekStartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (previous is null)
        {
            return (LeagueTier.Bronze, LeagueMovement.None);
        }

        var roomMemberPoints = await dbContext.LeagueMemberships
            .Where(m => m.RoomId == previous.RoomId)
            .OrderByDescending(m => m.Points)
            .ThenBy(m => m.UserId)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        var rank = roomMemberPoints.IndexOf(previous.UserId) + 1;
        var roomSize = roomMemberPoints.Count;
        var previousTier = previous.Room.Tier;

        if (previousTier != LeagueTier.Titanium && rank <= Math.Min(PromotionZoneSize, roomSize))
        {
            return (previousTier + 1, LeagueMovement.Promoted);
        }

        if (previousTier != LeagueTier.Bronze && rank > roomSize - Math.Min(RelegationZoneSize, roomSize))
        {
            return (previousTier - 1, LeagueMovement.Relegated);
        }

        return (previousTier, LeagueMovement.None);
    }

    private async Task<LeagueRoom> FindOrCreateOpenRoomAsync(LeagueTier tier, DateOnly weekStart, CancellationToken cancellationToken)
    {
        var candidateRooms = await dbContext.LeagueRooms
            .Where(r => r.Tier == tier && r.WeekStartDate == weekStart)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        foreach (var room in candidateRooms)
        {
            var memberCount = await dbContext.LeagueMemberships.CountAsync(m => m.RoomId == room.Id, cancellationToken);
            if (memberCount < RoomCapacity)
            {
                return room;
            }
        }

        var newRoom = new LeagueRoom
        {
            Id = Guid.NewGuid(),
            Tier = tier,
            WeekStartDate = weekStart,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.LeagueRooms.Add(newRoom);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newRoom;
    }

    private static DateOnly ComputeLocalWeekStart(int tzOffsetMinutes)
    {
        var local = DateTime.UtcNow.AddMinutes(tzOffsetMinutes);
        var daysSinceMonday = ((int)local.DayOfWeek + 6) % 7; // Monday=0 ... Sunday=6
        return DateOnly.FromDateTime(local.Date.AddDays(-daysSinceMonday));
    }

    private static (DateTime WeekStartUtc, DateTime WeekEndUtc) ComputeWeekUtcRange(int tzOffsetMinutes)
    {
        var weekStartLocal = ComputeLocalWeekStart(tzOffsetMinutes).ToDateTime(TimeOnly.MinValue);
        var weekStartUtc = DateTime.SpecifyKind(weekStartLocal.AddMinutes(-tzOffsetMinutes), DateTimeKind.Utc);
        return (weekStartUtc, weekStartUtc.AddDays(7));
    }
}
