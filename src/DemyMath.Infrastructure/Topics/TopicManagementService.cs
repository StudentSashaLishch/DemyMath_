using DemyMath.Application.Topics;
using DemyMath.Application.Topics.Models;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DemyMath.Application.Localization.Models;

namespace DemyMath.Infrastructure.Topics;

public class TopicManagementService : ITopicManagementService
{
    private readonly DemyMathDbContext _db;

    public TopicManagementService(DemyMathDbContext db)
    {
        _db = db;
    }

    public Task<TopicEditorDto> GetForCreateAsync(
        IReadOnlyCollection<string> supportedCultures)
    {
        var dto = new TopicEditorDto();

        foreach (var culture in supportedCultures)
        {
            dto.TitleTranslations.Add(new LocalizedValueDto
            {
                Culture = culture
            });

            dto.DescriptionTranslations.Add(new LocalizedValueDto
            {
                Culture = culture
            });
        }

        return Task.FromResult(dto);
    }

    public async Task<TopicEditorDto?> GetForEditAsync(
    int topicId,
    string culture,
    IReadOnlyCollection<string> supportedCultures,
    CancellationToken cancellationToken = default)
    {
        var topic = await _db.Topics
            .AsNoTracking()
            .Include(t => t.ContentItems)
            .Include(t => t.Prerequisites)
            .FirstOrDefaultAsync(
                t => t.Id == topicId &&
                     t.Version.Status == ContentVersionStatus.Draft,
                cancellationToken);

        if (topic is null)
            return null;

        var stringIds = topic.ContentItems
            .Where(c => c.TextStringId != null)
            .Select(c => c.TextStringId!)
            .Append(topic.TitleStringId)
            .Concat(topic.DescriptionStringId is null
                ? []
                : [topic.DescriptionStringId])
            .Distinct()
            .ToArray();

        var strings = await _db.LocalizedStrings
            .AsNoTracking()
            .Where(s =>
                s.VersionId == topic.VersionId &&
                stringIds.Contains(s.StringId))
            .ToListAsync(cancellationToken);

        List<LocalizedValueDto> GetTranslations(string? stringId)
        {
            return supportedCultures.Select(cultureCode =>
                new LocalizedValueDto
                {
                    Culture = cultureCode,
                    Value = strings.FirstOrDefault(s =>
                        s.StringId == stringId &&
                        s.Culture == cultureCode)?.Value ?? string.Empty
                }).ToList();
        }

        var result = new TopicEditorDto
        {
            Id = topic.Id,
            PositionX = topic.PositionX,
            PositionY = topic.PositionY,
            Icon = topic.Icon,
            TitleStringId = topic.TitleStringId,
            DescriptionStringId = topic.DescriptionStringId,
            TitleTranslations = GetTranslations(topic.TitleStringId),
            DescriptionTranslations = GetTranslations(topic.DescriptionStringId),
            PrerequisiteTopicIds = topic.Prerequisites
                .Select(p => p.PrerequisiteTopicId)
                .ToList()
        };

        foreach (var item in topic.ContentItems.OrderBy(c => c.Order))
        {
            result.ContentItems.Add(new TopicContentItemEditDto
            {
                Id = item.Id,
                Type = item.Type.ToString(),
                VideoUrl = item.VideoUrl,
                Translations = GetTranslations(item.TextStringId)
            });
        }

        return result;
    }

    public async Task<int> SaveAsync(
        TopicSaveDto dto,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var cultures = supportedCultures.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var topic = dto.Id.HasValue
            ? await _db.Topics
                .Include(t => t.Prerequisites)
                .Include(t => t.ContentItems)
                .FirstOrDefaultAsync(
                    t => t.Id == dto.Id.Value &&
                         t.Version.Status == ContentVersionStatus.Draft,
                    cancellationToken)
            : null;

        if (dto.Id.HasValue && topic is null)
            throw new InvalidOperationException("Topic not found.");

        var draft = topic is null
            ? await _db.LearningContentVersions
                .FirstOrDefaultAsync(
                    v => v.Status == ContentVersionStatus.Draft,
                    cancellationToken)
            : null;

        if (topic is null && draft is null)
            throw new InvalidOperationException("Draft not found.");

        var versionId = topic?.VersionId ?? draft!.Id;

        var titleStringId = topic?.TitleStringId
            ?? $"topic.title.{Guid.NewGuid():N}";

        var descriptionStringId = topic?.DescriptionStringId
            ?? $"topic.description.{Guid.NewGuid():N}";

        ValidateTranslations(dto.TitleTranslations, cultures, true);
        ValidateTranslations(dto.DescriptionTranslations, cultures, false);

        if (topic is null)
        {
            topic = new Topic
            {
                VersionId = versionId,
                TitleStringId = titleStringId,
                PositionX = dto.PositionX,
                PositionY = dto.PositionY,
                Icon = dto.Icon
            };

            _db.Topics.Add(topic);
        }
        else
        {
            topic.PositionX = dto.PositionX;
            topic.PositionY = dto.PositionY;
            topic.Icon = dto.Icon;
        }

        var oldStringIds = topic.ContentItems
            .Where(c => c.TextStringId != null)
            .Select(c => c.TextStringId!)
            .ToHashSet();

        if (topic.DescriptionStringId is not null)
            oldStringIds.Add(topic.DescriptionStringId);

        topic.Prerequisites.Clear();

        var prerequisiteIds = dto.PrerequisiteTopicIds
            .Distinct()
            .ToArray();

        if (prerequisiteIds.Contains(topic.Id) && topic.Id != 0)
            throw new InvalidOperationException(
                "A topic cannot be its own prerequisite.");

        var prerequisites = await _db.Topics
            .Where(t =>
                prerequisiteIds.Contains(t.Id) &&
                t.VersionId == versionId)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        if (prerequisites.Count != prerequisiteIds.Length)
            throw new InvalidOperationException(
                "One or more prerequisite topics are invalid.");

        foreach (var prerequisiteId in prerequisiteIds)
        {
            topic.Prerequisites.Add(new TopicPrerequisite
            {
                TopicId = topic.Id,
                PrerequisiteTopicId = prerequisiteId
            });
        }

        var oldItems = topic.ContentItems.ToList();
        _db.ContentItems.RemoveRange(oldItems);
        await _db.SaveChangesAsync(cancellationToken);

        topic.ContentItems.Clear();

        var order = 0;

        foreach (var itemDto in dto.ContentItems)
        {
            if (!Enum.TryParse<ContentItemType>(
                    itemDto.Type,
                    true,
                    out var type))
            {
                throw new InvalidOperationException(
                    "Invalid content item type.");
            }

            string? textStringId = null;
            string? videoUrl = null;

            if (type == ContentItemType.Text)
            {
                ValidateTranslations(
                    itemDto.Translations,
                    cultures,
                    true);

                var existingItem = itemDto.Id.HasValue
                    ? oldItems.FirstOrDefault(i => i.Id == itemDto.Id)
                    : null;

                if (itemDto.Id.HasValue && existingItem is null)
                    throw new InvalidOperationException(
                        "Invalid content item.");

                textStringId = existingItem?.TextStringId
                    ?? $"content.text.{Guid.NewGuid():N}";

                await SaveTranslationsAsync(
                    versionId,
                    textStringId,
                    itemDto.Translations,
                    cultures,
                    cancellationToken);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(itemDto.VideoUrl) ||
                    !Uri.TryCreate(
                        itemDto.VideoUrl,
                        UriKind.Absolute,
                        out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp &&
                     uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new InvalidOperationException(
                        "A valid video URL is required.");
                }

                videoUrl = itemDto.VideoUrl;
            }

            topic.ContentItems.Add(new ContentItem
            {
                TopicId = topic.Id,
                Type = type,
                Order = order++,
                TextStringId = textStringId,
                VideoUrl = videoUrl
            });
        }

        topic.DescriptionStringId =
            dto.DescriptionTranslations.Any(t =>
                !string.IsNullOrWhiteSpace(t.Value))
                ? descriptionStringId
                : null;

        await SaveTranslationsAsync(
            versionId,
            titleStringId,
            dto.TitleTranslations,
            cultures,
            cancellationToken);

        if (topic.DescriptionStringId is not null)
        {
            await SaveTranslationsAsync(
                versionId,
                descriptionStringId,
                dto.DescriptionTranslations,
                cultures,
                cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        await EnsureNoCyclesAsync(versionId, cancellationToken);

        oldStringIds.Add(titleStringId);
        oldStringIds.Add(descriptionStringId);

        await RemoveUnusedStringsAsync(
            versionId,
            oldStringIds,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return topic.Id;
    }

    public async Task DeleteAsync(
        int topicId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var topic = await _db.Topics
            .Include(t => t.ContentItems)
            .FirstOrDefaultAsync(
                t => t.Id == topicId &&
                     t.Version.Status == ContentVersionStatus.Draft,
                cancellationToken);

        if (topic is null)
            throw new InvalidOperationException("Topic not found.");

        var versionId = topic.VersionId;

        var stringIds = new HashSet<string>
        {
            topic.TitleStringId
        };

        if (topic.DescriptionStringId is not null)
            stringIds.Add(topic.DescriptionStringId);

        foreach (var item in topic.ContentItems)
        {
            if (item.TextStringId is not null)
                stringIds.Add(item.TextStringId);
        }

        var test = await _db.Tests
            .Where(t => t.TopicId == topicId)
            .Select(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (test != 0)
        {
            var questionIds = await _db.Questions
                .Where(q => q.TestId == test)
                .Select(q => q.Id)
                .ToListAsync(cancellationToken);

            var questionStringIds = await _db.Questions
                .Where(q => q.TestId == test)
                .Select(q => q.TextStringId)
                .ToListAsync(cancellationToken);

            stringIds.UnionWith(questionStringIds);

            var optionStringIds = await _db.QuestionOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .Select(o => o.TextStringId)
                .ToListAsync(cancellationToken);

            stringIds.UnionWith(optionStringIds);
        }

        var relatedPrerequisites = await _db.TopicPrerequisites
            .Where(p =>
                p.TopicId == topicId ||
                p.PrerequisiteTopicId == topicId)
            .ToListAsync(cancellationToken);

        _db.TopicPrerequisites.RemoveRange(relatedPrerequisites);
        _db.Topics.Remove(topic);

        await _db.SaveChangesAsync(cancellationToken);

        await RemoveUnusedStringsAsync(
            versionId,
            stringIds,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateTranslations(
        IReadOnlyCollection<LocalizedValueDto> translations,
        HashSet<string> supportedCultures,
        bool required)
    {
        var suppliedCultures = translations
            .Select(t => t.Culture)
            .ToArray();

        if (suppliedCultures.Any(c => !supportedCultures.Contains(c)) ||
            suppliedCultures.Distinct(
                StringComparer.OrdinalIgnoreCase).Count() !=
            suppliedCultures.Length)
        {
            throw new InvalidOperationException(
                "Invalid translation cultures.");
        }

        if (required && !translations.Any(t =>
                !string.IsNullOrWhiteSpace(t.Value)))
        {
            throw new InvalidOperationException(
                "At least one translation is required.");
        }
    }

    private async Task SaveTranslationsAsync(
        int versionId,
        string stringId,
        IEnumerable<LocalizedValueDto> translations,
        HashSet<string> supportedCultures,
        CancellationToken cancellationToken)
    {
        var values = translations
            .Where(t => supportedCultures.Contains(t.Culture))
            .ToDictionary(
                t => t.Culture,
                t => t.Value.Trim(),
                StringComparer.OrdinalIgnoreCase);

        var existing = await _db.LocalizedStrings
            .Where(s =>
                s.VersionId == versionId &&
                s.StringId == stringId)
            .ToListAsync(cancellationToken);

        foreach (var entry in existing)
        {
            if (!values.TryGetValue(entry.Culture, out var value) ||
                string.IsNullOrWhiteSpace(value))
            {
                _db.LocalizedStrings.Remove(entry);
            }
            else
            {
                entry.Value = value;
                values.Remove(entry.Culture);
            }
        }

        foreach (var (culture, value) in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _db.LocalizedStrings.Add(new LocalizedString
                {
                    VersionId = versionId,
                    StringId = stringId,
                    Culture = culture,
                    Value = value
                });
            }
        }
    }

    private async Task EnsureNoCyclesAsync(
        int versionId,
        CancellationToken cancellationToken)
    {
        var graph = await _db.TopicPrerequisites
            .Where(p => p.Topic.VersionId == versionId)
            .Select(p => new
            {
                p.TopicId,
                p.PrerequisiteTopicId
            })
            .ToListAsync(cancellationToken);

        var visited = new HashSet<int>();
        var active = new HashSet<int>();

        var adjacency = graph
            .GroupBy(e => e.TopicId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.PrerequisiteTopicId).ToList());

        bool HasCycle(int node)
        {
            if (active.Contains(node))
                return true;

            if (visited.Contains(node))
                return false;

            visited.Add(node);
            active.Add(node);

            if (adjacency.TryGetValue(node, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (HasCycle(dependency))
                        return true;
                }
            }

            active.Remove(node);
            return false;
        }

        foreach (var node in adjacency.Keys)
        {
            if (HasCycle(node))
                throw new InvalidOperationException(
                    "Prerequisites cannot contain cycles.");
        }
    }

    private async Task RemoveUnusedStringsAsync(
        int versionId,
        IEnumerable<string> candidates,
        CancellationToken cancellationToken)
    {
        var ids = candidates.Distinct().ToArray();

        if (ids.Length == 0)
            return;

        var used = new HashSet<string>();

        used.UnionWith(await _db.Topics
            .Where(t => t.VersionId == versionId &&
                        ids.Contains(t.TitleStringId))
            .Select(t => t.TitleStringId)
            .ToListAsync(cancellationToken));

        used.UnionWith(await _db.Topics
            .Where(t => t.VersionId == versionId &&
                        t.DescriptionStringId != null &&
                        ids.Contains(t.DescriptionStringId))
            .Select(t => t.DescriptionStringId!)
            .ToListAsync(cancellationToken));

        used.UnionWith(await _db.ContentItems
            .Where(c => c.Topic.VersionId == versionId &&
                        c.TextStringId != null &&
                        ids.Contains(c.TextStringId))
            .Select(c => c.TextStringId!)
            .ToListAsync(cancellationToken));

        used.UnionWith(await _db.Questions
            .Where(q => q.Test.Topic.VersionId == versionId &&
                        ids.Contains(q.TextStringId))
            .Select(q => q.TextStringId)
            .ToListAsync(cancellationToken));

        used.UnionWith(await _db.QuestionOptions
            .Where(o => o.Question.Test.Topic.VersionId == versionId &&
                        ids.Contains(o.TextStringId))
            .Select(o => o.TextStringId)
            .ToListAsync(cancellationToken));

        var unused = await _db.LocalizedStrings
            .Where(s =>
                s.VersionId == versionId &&
                ids.Contains(s.StringId) &&
                !used.Contains(s.StringId))
            .ToListAsync(cancellationToken);

        _db.LocalizedStrings.RemoveRange(unused);
    }
}