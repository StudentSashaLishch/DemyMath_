using DemyMath.Application.Localization;
using DemyMath.Application.Topics;
using DemyMath.Application.Topics.Models;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Topics;

public class TopicQueryService : ITopicQueryService
{
    private readonly DemyMathDbContext _dbContext;
    private readonly ILocalizedStringService _localizedStrings;

    public TopicQueryService(
        DemyMathDbContext dbContext,
        ILocalizedStringService localizedStrings)
    {
        _dbContext = dbContext;
        _localizedStrings = localizedStrings;
    }

    public async Task<TopicDetailsDto?> GetByIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published)
    {
        var topic = await _dbContext.Topics
            .AsNoTracking()
            .Where(t =>
                t.Id == topicId &&
                t.Version.Status == versionStatus)
            .Select(t => new
            {
                t.Id,
                t.VersionId,
                t.TitleStringId,
                t.DescriptionStringId,
                t.PositionX,
                t.PositionY,
                t.Icon,
                PrerequisiteTopicIds = t.Prerequisites
                    .Where(p => p.PrerequisiteTopic.VersionId == t.VersionId)
                    .Select(p => p.PrerequisiteTopicId)
                    .ToArray(),
                ContentItems = t.ContentItems
                    .OrderBy(c => c.Order)
                    .Select(c => new
                    {
                        c.Id,
                        c.Type,
                        c.Order,
                        c.TextStringId,
                        c.VideoUrl
                    })
                    .ToArray()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topic is null)
        {
            return null;
        }

        var hasTest = await _dbContext.Tests
            .AsNoTracking()
            .AnyAsync(t => t.TopicId == topicId, cancellationToken);

        var stringIds = topic.ContentItems
            .Where(c => c.TextStringId != null)
            .Select(c => c.TextStringId!)
            .Append(topic.TitleStringId)
            .Concat(topic.DescriptionStringId is null
                ? []
                : [topic.DescriptionStringId])
            .ToArray();

        var strings = await _localizedStrings.GetManyAsync(
            stringIds,
            culture,
            topic.VersionId,
            cancellationToken);

        return new TopicDetailsDto
        {
            Id = topic.Id,
            Title = strings[topic.TitleStringId],
            Description = topic.DescriptionStringId is null
                ? null
                : strings[topic.DescriptionStringId],
            PositionX = topic.PositionX,
            PositionY = topic.PositionY,
            Icon = topic.Icon,
            HasTest = hasTest,
            PrerequisiteTopicIds = topic.PrerequisiteTopicIds,
            ContentItems = topic.ContentItems
                .Select(c => new TopicContentItemDto
                {
                    Id = c.Id,
                    Type = c.Type.ToString(),
                    Order = c.Order,
                    Text = c.TextStringId is null
                        ? null
                        : strings[c.TextStringId],
                    VideoUrl = c.VideoUrl
                })
                .ToArray()
        };
    }

    public async Task<IReadOnlyList<TopicDetailsDto>> GetAllAsync(
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published)
    {
        var topicIds = await _dbContext.Topics
            .AsNoTracking()
            .Where(t => t.Version.Status == versionStatus)
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var topics = new List<TopicDetailsDto>(topicIds.Count);

        foreach (var topicId in topicIds)
        {
            var topic = await GetByIdAsync(
                topicId,
                culture,
                cancellationToken,
                versionStatus);

            if (topic is not null)
            {
                topics.Add(topic);
            }
        }

        return topics;
    }
}