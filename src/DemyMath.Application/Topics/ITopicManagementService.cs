using DemyMath.Application.Topics.Models;

namespace DemyMath.Application.Topics;

public interface ITopicManagementService
{
    Task<TopicEditorDto?> GetForEditAsync(
        int topicId,
        string culture,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default);

    Task<TopicEditorDto> GetForCreateAsync(
        IReadOnlyCollection<string> supportedCultures);

    Task<int> SaveAsync(
        TopicSaveDto dto,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int topicId,
        CancellationToken cancellationToken = default);
}