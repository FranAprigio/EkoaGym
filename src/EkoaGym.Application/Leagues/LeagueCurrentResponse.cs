using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Leagues;

public record LeagueMemberResponse(Guid UserId, string Name, int Rank, decimal Points, bool IsCurrentUser);

public record LeagueCurrentResponse(
    LeagueTier Tier,
    Guid RoomId,
    int Rank,
    decimal Points,
    IReadOnlyList<LeagueMemberResponse> Standings,
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    LeagueMovement PreviousMovement,
    int PromotionZoneSize,
    int RelegationZoneSize);
