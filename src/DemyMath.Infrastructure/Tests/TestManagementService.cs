using DemyMath.Application.Tests;
using DemyMath.Application.Tests.Models;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Tests;

public class TestManagementService : ITestManagementService
{
    private readonly DemyMathDbContext _db;

    public TestManagementService(DemyMathDbContext db)
    {
        _db = db;
    }

    public async Task<TestEditorDto?> GetForEditAsync(
    int topicId,
    IReadOnlyCollection<string> supportedCultures,
    CancellationToken cancellationToken = default)
    {
        var topic = await _db.Topics
            .AsNoTracking()
            .Where(t =>
                t.Id == topicId &&
                t.Version.Status == ContentVersionStatus.Draft)
            .Select(t => new
            {
                t.Id,
                t.VersionId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topic is null)
            return null;

        var result = new TestEditorDto
        {
            TopicId = topicId
        };

        var test = await _db.Tests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.TopicId == topicId,
                cancellationToken);

        // Тема існує, але тесту ще немає — повертаємо порожній редактор.
        if (test is null)
            return result;

        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => q.TestId == test.Id)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(q => q.Id).ToArray();

        var options = await _db.QuestionOptions
            .AsNoTracking()
            .Where(o => questionIds.Contains(o.QuestionId))
            .OrderBy(o => o.Order)
            .ToListAsync(cancellationToken);

        var stringIds = questions
            .Select(q => q.TextStringId)
            .Concat(options.Select(o => o.TextStringId))
            .Distinct()
            .ToArray();

        var strings = await _db.LocalizedStrings
            .AsNoTracking()
            .Where(s =>
                s.VersionId == topic.VersionId &&
                stringIds.Contains(s.StringId))
            .ToListAsync(cancellationToken);

        List<LocalizedValueDto> GetTranslations(string stringId)
        {
            return supportedCultures.Select(culture => new LocalizedValueDto
            {
                Culture = culture,
                Value = strings.FirstOrDefault(s =>
                    s.StringId == stringId &&
                    s.Culture == culture)?.Value ?? string.Empty
            }).ToList();
        }

        foreach (var question in questions)
        {
            var dto = new QuestionEditorDto
            {
                Id = question.Id,
                Translations = GetTranslations(question.TextStringId)
            };

            switch (question)
            {
                case MultipleChoiceQuestion multipleChoice:
                    dto.Type = "MultipleChoice";
                    dto.Options = options
                        .Where(o => o.QuestionId == question.Id)
                        .Select(o => new QuestionOptionEditorDto
                        {
                            Id = o.Id,
                            IsCorrect = o.IsCorrect,
                            Translations = GetTranslations(o.TextStringId)
                        })
                        .ToList();
                    break;

                case NumericQuestion numeric:
                    dto.Type = "Numeric";
                    dto.CorrectAnswer = numeric.CorrectAnswer;
                    dto.Tolerance = numeric.Tolerance;
                    break;

                case CoordinatePointQuestion coordinate:
                    dto.Type = "CoordinatePoint";
                    dto.CorrectX = coordinate.CorrectX;
                    dto.CorrectY = coordinate.CorrectY;
                    dto.Tolerance = coordinate.Tolerance;
                    dto.MinX = coordinate.MinX;
                    dto.MaxX = coordinate.MaxX;
                    dto.MinY = coordinate.MinY;
                    dto.MaxY = coordinate.MaxY;
                    dto.GridStep = coordinate.GridStep;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported question type: {question.GetType().Name}");
            }

            result.Questions.Add(dto);
        }

        return result;
    }

    public async Task SaveAsync(
        TestSaveDto dto,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var cultures = supportedCultures.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        var topic = await _db.Topics
            .FirstOrDefaultAsync(
                t => t.Id == dto.TopicId &&
                     t.Version.Status == ContentVersionStatus.Draft,
                cancellationToken);

        if (topic is null)
            throw new InvalidOperationException("Draft topic not found.");

        var test = await _db.Tests
            .FirstOrDefaultAsync(
                t => t.TopicId == dto.TopicId,
                cancellationToken);

        if (test is null)
        {
            test = new Test
            {
                TopicId = dto.TopicId
            };

            _db.Tests.Add(test);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var oldQuestions = await _db.Questions
            .Where(q => q.TestId == test.Id)
            .ToListAsync(cancellationToken);

        var oldQuestionIds = oldQuestions.Select(q => q.Id).ToArray();

        var oldOptions = await _db.QuestionOptions
            .Where(o => oldQuestionIds.Contains(o.QuestionId))
            .ToListAsync(cancellationToken);

        var oldQuestionStrings = oldQuestions.ToDictionary(
            q => q.Id,
            q => q.TextStringId);

        var oldOptionStrings = oldOptions.ToDictionary(
            o => o.Id,
            o => o.TextStringId);

        var candidateStringIds = oldQuestionStrings.Values
            .Concat(oldOptionStrings.Values)
            .ToHashSet();

        ValidateQuestionIds(dto, oldQuestionIds);
        ValidateOptionIds(dto, oldOptions);

        foreach (var question in dto.Questions)
        {
            ValidateTranslations(question.Translations, cultures, true);

            switch (question.Type)
            {
                case "MultipleChoice":
                    if (question.Options.Count < 2 ||
                        question.Options.Count(o => o.IsCorrect) != 1)
                    {
                        throw new InvalidOperationException(
                            "A multiple-choice question must have at least two options and exactly one correct answer.");
                    }

                    foreach (var option in question.Options)
                        ValidateTranslations(option.Translations, cultures, true);

                    break;

                case "Numeric":
                    if (question.Tolerance < 0)
                    {
                        throw new InvalidOperationException(
                            "Tolerance cannot be negative.");
                    }

                    break;

                case "CoordinatePoint":
                    if (question.Tolerance < 0 ||
                        question.MinX >= question.MaxX ||
                        question.MinY >= question.MaxY ||
                        question.GridStep <= 0 ||
                        question.CorrectX < question.MinX ||
                        question.CorrectX > question.MaxX ||
                        question.CorrectY < question.MinY ||
                        question.CorrectY > question.MaxY)
                    {
                        throw new InvalidOperationException(
                            "Invalid coordinate question parameters.");
                    }

                    break;

                default:
                    throw new InvalidOperationException(
                        "Unsupported question type.");
            }
        }

        _db.Questions.RemoveRange(oldQuestions);
        await _db.SaveChangesAsync(cancellationToken);

        var order = 0;

        foreach (var questionDto in dto.Questions)
        {
            var textStringId = questionDto.Id.HasValue
    ? oldQuestionStrings[questionDto.Id.Value]
    : $"question.text.{Guid.NewGuid():N}";

            Question question = questionDto.Type switch
            {
                "MultipleChoice" => new MultipleChoiceQuestion
                {
                    TextStringId = textStringId
                },

                "Numeric" => new NumericQuestion
                {
                    TextStringId = textStringId,
                    CorrectAnswer = questionDto.CorrectAnswer,
                    Tolerance = questionDto.Tolerance
                },

                "CoordinatePoint" => new CoordinatePointQuestion
                {
                    TextStringId = textStringId,
                    CorrectX = questionDto.CorrectX,
                    CorrectY = questionDto.CorrectY,
                    Tolerance = questionDto.Tolerance,
                    MinX = questionDto.MinX,
                    MaxX = questionDto.MaxX,
                    MinY = questionDto.MinY,
                    MaxY = questionDto.MaxY,
                    GridStep = questionDto.GridStep
                },

                _ => throw new InvalidOperationException(
                    "Unsupported question type.")
            };

            question.TestId = test.Id;
            question.Order = order++;

            _db.Questions.Add(question);

            await SaveTranslationsAsync(
                topic.VersionId,
                textStringId,
                questionDto.Translations,
                cultures,
                cancellationToken);

            if (question is MultipleChoiceQuestion multipleChoice)
            {
                var optionOrder = 0;

                foreach (var optionDto in questionDto.Options)
                {
                    var optionStringId = optionDto.Id.HasValue
                        ? oldOptionStrings[optionDto.Id.Value]
                        : $"question.option.{Guid.NewGuid():N}";

                    multipleChoice.Options.Add(new QuestionOption
                    {
                        Question = multipleChoice,
                        TextStringId = optionStringId,
                        IsCorrect = optionDto.IsCorrect,
                        Order = optionOrder++
                    });

                    await SaveTranslationsAsync(
                        topic.VersionId,
                        optionStringId,
                        optionDto.Translations,
                        cultures,
                        cancellationToken);
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await RemoveUnusedStringsAsync(
            topic.VersionId,
            candidateStringIds,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int topicId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var test = await _db.Tests
            .Include(t => t.Topic)
            .FirstOrDefaultAsync(
                t => t.TopicId == topicId &&
                     t.Topic.Version.Status == ContentVersionStatus.Draft,
                cancellationToken);

        if (test is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var questions = await _db.Questions
            .Where(q => q.TestId == test.Id)
            .ToListAsync(cancellationToken);

        var questionIds = questions.Select(q => q.Id).ToArray();

        var options = await _db.QuestionOptions
            .Where(o => questionIds.Contains(o.QuestionId))
            .ToListAsync(cancellationToken);

        var stringIds = questions
            .Select(q => q.TextStringId)
            .Concat(options.Select(o => o.TextStringId))
            .Distinct()
            .ToArray();

        _db.Tests.Remove(test);
        await _db.SaveChangesAsync(cancellationToken);

        await RemoveUnusedStringsAsync(
            test.Topic.VersionId,
            stringIds,
            cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void ValidateQuestionIds(
        TestSaveDto dto,
        IReadOnlyCollection<int> existingIds)
    {
        var suppliedIds = dto.Questions
            .Where(q => q.Id.HasValue)
            .Select(q => q.Id!.Value)
            .ToArray();

        if (suppliedIds.Distinct().Count() != suppliedIds.Length ||
            suppliedIds.Any(id => !existingIds.Contains(id)))
        {
            throw new InvalidOperationException(
                "One or more question IDs are invalid.");
        }
    }

    private static void ValidateOptionIds(
        TestSaveDto dto,
        IReadOnlyCollection<QuestionOption> existingOptions)
    {
        var suppliedOptions = dto.Questions
            .SelectMany(q => q.Options)
            .Where(o => o.Id.HasValue)
            .ToArray();

        var suppliedIds = suppliedOptions
            .Select(o => o.Id!.Value)
            .ToArray();

        if (suppliedIds.Distinct().Count() != suppliedIds.Length)
            throw new InvalidOperationException("Duplicate option IDs.");

        var existingById = existingOptions.ToDictionary(o => o.Id);

        foreach (var option in suppliedOptions)
        {
            if (!existingById.TryGetValue(option.Id!.Value, out var existing))
                throw new InvalidOperationException("Invalid option ID.");

            var question = dto.Questions.FirstOrDefault(q =>
                q.Options.Contains(option));

            if (question?.Id != existing.QuestionId)
                throw new InvalidOperationException(
                    "An option cannot be moved to another question.");
        }
    }

    private static void ValidateTranslations(
        IReadOnlyCollection<LocalizedValueDto> translations,
        HashSet<string> supportedCultures,
        bool required)
    {
        var suppliedCultures = translations.Select(t => t.Culture).ToArray();

        if (suppliedCultures.Any(c => !supportedCultures.Contains(c)) ||
            suppliedCultures.Distinct(
                StringComparer.OrdinalIgnoreCase).Count() != suppliedCultures.Length)
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