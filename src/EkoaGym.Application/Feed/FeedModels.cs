using EkoaGym.Domain.Enums;

namespace EkoaGym.Application.Feed;

public record MuscleGroupVolume(MuscleGroup MuscleGroup, decimal VolumeKg);

public record FeedCardResponse(
    Guid SessionId,
    Guid UserId,
    string UserName,
    DateTime StartedAt,
    DateTime EndedAt,
    decimal TotalTonnageKg,
    int ActiveMinutes,
    bool HasPr,
    IReadOnlyList<MuscleGroupVolume> MuscleGroupVolumes,
    int ReactionCount,
    bool ReactedByCurrentUser);

public record FeedReactionResponse(Guid SessionId, bool Reacted, int ReactionCount);
