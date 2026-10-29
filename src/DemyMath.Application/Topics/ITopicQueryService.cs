using DemyMath.Application.Topics.Models;

namespace DemyMath.Application.Topics;

public interface ITopicQueryService
{
    Task<TopicDetailsDto?> GetByIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopicDetailsDto>> GetAllAsync(
        string culture,
        CancellationToken cancellationToken = default);
}