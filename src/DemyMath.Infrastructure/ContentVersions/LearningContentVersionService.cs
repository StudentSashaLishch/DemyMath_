using DemyMath.Application.ContentVersions;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.ContentVersions;

public class LearningContentVersionService : ILearningContentVersionService
{
    private readonly DemyMathDbContext _dbContext;

    public LearningContentVersionService(DemyMathDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> EnsureDraftAsync(
        CancellationToken cancellationToken = default)
    {
        var existingDraft = await _dbContext.LearningContentVersions
            .AsNoTracking()
            .Where(v => v.Status == ContentVersionStatus.Draft)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingDraft.HasValue)
        {
            return existingDraft.Value;
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var published = await _dbContext.LearningContentVersions
            .AsNoTracking()
            .Where(v => v.Status == ContentVersionStatus.Published)
            .OrderByDescending(v => v.PublishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var draft = new LearningContentVersion
        {
            Status = ContentVersionStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.LearningContentVersions.Add(draft);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (published is not null)
        {
            await CopyVersionAsync(
                published.Id,
                draft.Id,
                cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return draft.Id;
    }

    private async Task CopyVersionAsync(
        int sourceVersionId,
        int targetVersionId,
        CancellationToken cancellationToken)
    {
        var sourceTopics = await _dbContext.Topics
            .AsNoTracking()
            .Where(t => t.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourceLocalizedStrings = await _dbContext.LocalizedStrings
            .AsNoTracking()
            .Where(s => s.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourceContentItems = await _dbContext.ContentItems
            .AsNoTracking()
            .Where(c => c.Topic.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourcePrerequisites = await _dbContext.TopicPrerequisites
            .AsNoTracking()
            .Where(p => p.Topic.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourceTests = await _dbContext.Tests
            .AsNoTracking()
            .Where(t => t.Topic.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourceQuestions = await _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.Test.Topic.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        var sourceQuestionOptions = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => o.Question.Test.Topic.VersionId == sourceVersionId)
            .ToListAsync(cancellationToken);

        // Topics
        var topicMap = new Dictionary<int, Topic>();

        foreach (var sourceTopic in sourceTopics)
        {
            var targetTopic = new Topic
            {
                VersionId = targetVersionId,
                TitleStringId = sourceTopic.TitleStringId,
                DescriptionStringId = sourceTopic.DescriptionStringId,
                PositionX = sourceTopic.PositionX,
                PositionY = sourceTopic.PositionY,
                Icon = sourceTopic.Icon,
                ProgressKey = sourceTopic.ProgressKey
            };

            _dbContext.Topics.Add(targetTopic);
            topicMap.Add(sourceTopic.Id, targetTopic);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Topic prerequisites
        foreach (var sourcePrerequisite in sourcePrerequisites)
        {
            if (!topicMap.TryGetValue(
                    sourcePrerequisite.TopicId,
                    out var targetTopic) ||
                !topicMap.TryGetValue(
                    sourcePrerequisite.PrerequisiteTopicId,
                    out var targetPrerequisite))
            {
                continue;
            }

            _dbContext.TopicPrerequisites.Add(new TopicPrerequisite
            {
                TopicId = targetTopic.Id,
                PrerequisiteTopicId = targetPrerequisite.Id
            });
        }

        // Content items
        foreach (var sourceItem in sourceContentItems)
        {
            if (!topicMap.TryGetValue(sourceItem.TopicId, out var targetTopic))
            {
                continue;
            }

            _dbContext.ContentItems.Add(new ContentItem
            {
                TopicId = targetTopic.Id,
                Type = sourceItem.Type,
                Order = sourceItem.Order,
                TextStringId = sourceItem.TextStringId,
                VideoUrl = sourceItem.VideoUrl
            });
        }

        // Tests
        var sourceTestsByTopic = sourceTests.ToDictionary(t => t.TopicId);
        var testMap = new Dictionary<int, Test>();

        foreach (var sourceTopic in sourceTopics)
        {
            if (!sourceTestsByTopic.TryGetValue(
                    sourceTopic.Id,
                    out var sourceTest) ||
                !topicMap.TryGetValue(sourceTopic.Id, out var targetTopic))
            {
                continue;
            }

            var targetTest = new Test
            {
                TopicId = targetTopic.Id
            };

            _dbContext.Tests.Add(targetTest);
            testMap.Add(sourceTest.Id, targetTest);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Questions
        var questionMap = new Dictionary<int, Question>();

        foreach (var sourceQuestion in sourceQuestions)
        {
            if (!testMap.TryGetValue(
                    sourceQuestion.TestId,
                    out var targetTest))
            {
                continue;
            }

            Question targetQuestion = sourceQuestion switch
            {
                MultipleChoiceQuestion => new MultipleChoiceQuestion
                {
                    TestId = targetTest.Id,
                    TextStringId = sourceQuestion.TextStringId,
                    Order = sourceQuestion.Order
                },

                NumericQuestion numeric => new NumericQuestion
                {
                    TestId = targetTest.Id,
                    TextStringId = numeric.TextStringId,
                    Order = numeric.Order,
                    CorrectAnswer = numeric.CorrectAnswer,
                    Tolerance = numeric.Tolerance
                },

                CoordinatePointQuestion point => new CoordinatePointQuestion
                {
                    TestId = targetTest.Id,
                    TextStringId = point.TextStringId,
                    Order = point.Order,
                    CorrectX = point.CorrectX,
                    CorrectY = point.CorrectY,
                    Tolerance = point.Tolerance,
                    MinX = point.MinX,
                    MaxX = point.MaxX,
                    MinY = point.MinY,
                    MaxY = point.MaxY,
                    GridStep = point.GridStep
                },

                _ => throw new InvalidOperationException(
                    $"Unsupported question type: {sourceQuestion.GetType().Name}")
            };

            _dbContext.Questions.Add(targetQuestion);
            questionMap.Add(sourceQuestion.Id, targetQuestion);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Question options
        foreach (var sourceOption in sourceQuestionOptions)
        {
            if (!questionMap.TryGetValue(
                    sourceOption.QuestionId,
                    out var targetQuestion) ||
                targetQuestion is not MultipleChoiceQuestion targetMultipleChoice)
            {
                continue;
            }

            _dbContext.QuestionOptions.Add(new QuestionOption
            {
                QuestionId = targetMultipleChoice.Id,
                TextStringId = sourceOption.TextStringId,
                IsCorrect = sourceOption.IsCorrect,
                Order = sourceOption.Order
            });
        }

        // Localized strings
        foreach (var sourceString in sourceLocalizedStrings)
        {
            _dbContext.LocalizedStrings.Add(new LocalizedString
            {
                VersionId = targetVersionId,
                StringId = sourceString.StringId,
                Culture = sourceString.Culture,
                Value = sourceString.Value
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishDraftAsync(
    CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var draft = await _dbContext.LearningContentVersions
            .FirstOrDefaultAsync(
                v => v.Status == ContentVersionStatus.Draft,
                cancellationToken);

        if (draft is null)
        {
            throw new InvalidOperationException("No draft version exists.");
        }

        var currentProgressKeys = await _dbContext.Topics
            .Where(t => t.VersionId == draft.Id)
            .Select(t => t.ProgressKey)
            .ToListAsync(cancellationToken);

        var published = await _dbContext.LearningContentVersions
            .FirstOrDefaultAsync(
                v => v.Status == ContentVersionStatus.Published,
                cancellationToken);

        if (published is not null)
        {
            _dbContext.LearningContentVersions.Remove(published);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        draft.Status = ContentVersionStatus.Published;
        draft.PublishedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var obsoleteProgress = _dbContext.UserTopicProgress
            .Where(p => !currentProgressKeys.Contains(p.ProgressKey));

        await obsoleteProgress.ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}