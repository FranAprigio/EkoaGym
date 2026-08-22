using EkoaGym.Application.Common;

namespace EkoaGym.Application.Feed;

public interface IFeedService
{
    Task<PagedResult<FeedCardResponse>> GetFeedAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    // Toggle: reacting again removes the reaction rather than double-counting (M-10.6).
    Task<FeedReactionResponse> ToggleReactionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
}
