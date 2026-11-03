using DemyMath.Domain.Entities;
using DemyMath.Application.Topics.Models;

namespace DemyMath.Application.Topics;

public interface ITopicQueryService
{
    Task<TopicDetailsDto?> GetByIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published);

    Task<IReadOnlyList<TopicDetailsDto>> GetAllAsync(
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published);
}