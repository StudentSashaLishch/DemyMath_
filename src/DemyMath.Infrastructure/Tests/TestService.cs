using System.Globalization;
using DemyMath.Application.Localization;
using DemyMath.Application.Tests;
using DemyMath.Application.Tests.Models;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Tests;

public class TestService : ITestService
{
    private readonly DemyMathDbContext _dbContext;
    private readonly ILocalizedStringService _localizedStrings;

    public TestService(
        DemyMathDbContext dbContext,
        ILocalizedStringService localizedStrings)
    {
        _dbContext = dbContext;
        _localizedStrings = localizedStrings;
    }

    public async Task<TestDto?> GetByTopicIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var test = await _dbContext.Tests
            .AsNoTracking()
            .Where(t => t.TopicId == topicId)
            .Select(t => new
            {
                t.Id,
                TopicTitleStringId = t.Topic.TitleStringId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (test is null)
        {
            return null;
        }

        var questions = await _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.TestId == test.Id)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var multipleChoiceIds = questions
            .OfType<MultipleChoiceQuestion>()
            .Select(q => q.Id)
            .ToArray();

        var options = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => multipleChoiceIds.Contains(o.QuestionId))
            .OrderBy(o => o.Order)
            .ToListAsync(cancellationToken);

        var stringIds = questions
            .Select(q => q.TextStringId)
            .Concat(options.Select(o => o.TextStringId))
            .Append(test.TopicTitleStringId)
            .ToArray();

        var strings = await _localizedStrings.GetManyAsync(
            stringIds,
            culture,
            cancellationToken);

        var optionsByQuestion = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var questionDtos = questions.Select(question =>
        {
            var dto = new QuestionDto
            {
                Id = question.Id,
                Text = strings[question.TextStringId],
                Order = question.Order,
                Type = question switch
                {
                    MultipleChoiceQuestion => "MultipleChoice",
                    NumericQuestion => "Numeric",
                    CoordinatePointQuestion => "CoordinatePoint",
                    _ => throw new InvalidOperationException(
                        $"Unsupported question type: {question.GetType().Name}")
                },
                Options = question is MultipleChoiceQuestion
                    ? optionsByQuestion[question.Id]
                        .Select(o => new QuestionOptionDto
                        {
                            Id = o.Id,
                            Text = strings[o.TextStringId],
                            Order = o.Order
                        })
                        .ToArray()
                    : []
            };

            if (question is CoordinatePointQuestion pointQuestion)
            {
                return new QuestionDto
                {
                    Id = dto.Id,
                    Text = dto.Text,
                    Order = dto.Order,
                    Type = dto.Type,
                    Options = dto.Options,
                    MinX = pointQuestion.MinX,
                    MaxX = pointQuestion.MaxX,
                    MinY = pointQuestion.MinY,
                    MaxY = pointQuestion.MaxY,
                    GridStep = pointQuestion.GridStep
                };
            }

            return dto;
        }).ToArray();

        return new TestDto
        {
            Id = test.Id,
            TopicId = topicId,
            TopicTitle = strings[test.TopicTitleStringId],
            Questions = questionDtos
        };
    }

    public async Task<TestResultDto?> SubmitAsync(
        int topicId,
        TestSubmissionDto submission,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var testId = await _dbContext.Tests
            .AsNoTracking()
            .Where(t => t.TopicId == topicId)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (testId is null)
        {
            return null;
        }

        var questions = await _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.TestId == testId.Value)
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

        var multipleChoiceIds = questions
            .OfType<MultipleChoiceQuestion>()
            .Select(q => q.Id)
            .ToArray();

        var options = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => multipleChoiceIds.Contains(o.QuestionId))
            .ToListAsync(cancellationToken);

        var answersByQuestion = submission.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var stringIds = questions
            .Select(q => q.TextStringId)
            .ToArray();

        var strings = await _localizedStrings.GetManyAsync(
            stringIds,
            culture,
            cancellationToken);

        var results = new List<QuestionResultDto>(questions.Count);

        foreach (var question in questions)
        {
            var isCorrect = false;

            if (answersByQuestion.TryGetValue(
                    question.Id, out var submittedAnswers) &&
                submittedAnswers.Length == 1)
            {
                var answer = submittedAnswers[0];

                isCorrect = question switch
                {
                    MultipleChoiceQuestion multipleChoice =>
                        IsCorrectOption(
                            multipleChoice,
                            answer.SelectedOptionId,
                            options),

                    NumericQuestion numeric =>
                        IsCorrectNumber(
                            numeric,
                            answer.NumericAnswer),

                    CoordinatePointQuestion point =>
                        IsCorrectPoint(
                            point,
                            answer.X,
                            answer.Y),

                    _ => false
                };
            }

            results.Add(new QuestionResultDto
            {
                QuestionId = question.Id,
                Text = strings[question.TextStringId],
                IsCorrect = isCorrect
            });
        }

        return new TestResultDto
        {
            CorrectAnswers = results.Count(r => r.IsCorrect),
            TotalQuestions = results.Count,
            Questions = results
        };
    }

    private static bool IsCorrectOption(
        MultipleChoiceQuestion question,
        int? selectedOptionId,
        IReadOnlyCollection<QuestionOption> options)
    {
        if (selectedOptionId is null)
        {
            return false;
        }

        return options.Any(o =>
            o.QuestionId == question.Id &&
            o.Id == selectedOptionId.Value &&
            o.IsCorrect);
    }

    private static bool IsCorrectNumber(
        NumericQuestion question,
        string? submittedValue)
    {
        if (!TryParseDecimal(submittedValue, out var value))
        {
            return false;
        }

        return Math.Abs(value - question.CorrectAnswer)
            <= question.Tolerance;
    }

    private static bool IsCorrectPoint(
        CoordinatePointQuestion question,
        string? submittedX,
        string? submittedY)
    {
        if (!TryParseDecimal(submittedX, out var x) ||
            !TryParseDecimal(submittedY, out var y))
        {
            return false;
        }

        if (x < question.MinX || x > question.MaxX ||
            y < question.MinY || y > question.MaxY)
        {
            return false;
        }

        return Math.Abs(x - question.CorrectX) <= question.Tolerance &&
               Math.Abs(y - question.CorrectY) <= question.Tolerance;
    }

    private static bool TryParseDecimal(
        string? input,
        out decimal value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }
}