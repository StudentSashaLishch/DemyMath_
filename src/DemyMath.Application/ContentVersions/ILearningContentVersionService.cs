namespace DemyMath.Application.ContentVersions;

public interface ILearningContentVersionService
{
    Task<int> EnsureDraftAsync(
        CancellationToken cancellationToken = default);

    Task PublishDraftAsync(
        CancellationToken cancellationToken = default);
}