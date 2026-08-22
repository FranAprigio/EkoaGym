namespace EkoaGym.Mobile.Models;

public record MuscleGroupVolume(string MuscleGroup, decimal VolumeKg);

public record FeedCard(
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

public record FeedReactionResult(Guid SessionId, bool Reacted, int ReactionCount);
