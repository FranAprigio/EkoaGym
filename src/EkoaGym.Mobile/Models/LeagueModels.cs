namespace EkoaGym.Mobile.Models;

public record LeagueMember(Guid UserId, string Name, int Rank, decimal Points, bool IsCurrentUser);

public record LeagueCurrent(
    string Tier,
    Guid RoomId,
    int Rank,
    decimal Points,
    IReadOnlyList<LeagueMember> Standings,
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    string PreviousMovement,
    int PromotionZoneSize,
    int RelegationZoneSize);
