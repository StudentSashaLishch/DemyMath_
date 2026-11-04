using DemyMath.Application.Progress.Models;

namespace DemyMath.Application.Progress;

public interface IUserProgressService
{
    Task MarkCompletedAsync(
        string userId,
        int topicId,
        CancellationToken cancellationToken = default);

    Task MarkCompletedIfNoTestAsync(
        string userId,
        int topicId,
        CancellationToken cancellationToken = default);

    Task<HashSet<Guid>> GetCompletedProgressKeysAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<UserProgressSummaryDto> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task ResetAsync(
        string userId,
        CancellationToken cancellationToken = default);
}