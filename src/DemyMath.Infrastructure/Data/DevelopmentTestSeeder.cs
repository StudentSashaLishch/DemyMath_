using DemyMath.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Data;

public sealed class DevelopmentTestSeeder
{
    private readonly DemyMathDbContext _dbContext;

    public DevelopmentTestSeeder(DemyMathDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var topic = await _dbContext.Topics
            .FirstOrDefaultAsync(
                t => t.TitleStringId == "topic.arithmetic.title",
                cancellationToken);

        if (topic is null)
        {
            return;
        }

        var multipleChoice = new MultipleChoiceQuestion
        {
            TextStringId = "test.arithmetic.q1",
            Order = 1
        };

        multipleChoice.Options =
        [
            new QuestionOption
            {
                TextStringId = "test.arithmetic.q1.option1",
                Order = 1,
                IsCorrect = false,
                Question = multipleChoice
            },
            new QuestionOption
            {
                TextStringId = "test.arithmetic.q1.option2",
                Order = 2,
                IsCorrect = true,
                Question = multipleChoice
            },
            new QuestionOption
            {
                TextStringId = "test.arithmetic.q1.option3",
                Order = 3,
                IsCorrect = false,
                Question = multipleChoice
            }
        ];

        var numeric = new NumericQuestion
        {
            TextStringId = "test.arithmetic.q2",
            Order = 2,
            CorrectAnswer = 7,
            Tolerance = 0
        };

        var coordinate = new CoordinatePointQuestion
        {
            TextStringId = "test.arithmetic.q3",
            Order = 3,
            CorrectX = 2,
            CorrectY = 3,
            Tolerance = 0,
            MinX = -5,
            MaxX = 5,
            MinY = -5,
            MaxY = 5,
            GridStep = 1
        };

        var test = new Test
        {
            TopicId = topic.Id,
            Questions =
            [
                multipleChoice,
                numeric,
                coordinate
            ]
        };

        var strings = new[]
        {
            new LocalizedString
            {
                StringId = "test.arithmetic.q1",
                Culture = "uk",
                Value = "Скільки буде 2 + 2?"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1",
                Culture = "en",
                Value = "What is 2 + 2?"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option1",
                Culture = "uk",
                Value = "3"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option1",
                Culture = "en",
                Value = "3"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option2",
                Culture = "uk",
                Value = "4"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option2",
                Culture = "en",
                Value = "4"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option3",
                Culture = "uk",
                Value = "5"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q1.option3",
                Culture = "en",
                Value = "5"
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q2",
                Culture = "uk",
                Value = "Обчисліть: 10 - 3."
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q2",
                Culture = "en",
                Value = "Calculate: 10 - 3."
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q3",
                Culture = "uk",
                Value = "Виберіть точку з координатами (2; 3)."
            },
            new LocalizedString
            {
                StringId = "test.arithmetic.q3",
                Culture = "en",
                Value = "Select the point with coordinates (2, 3)."
            }
        };

        await _dbContext.LocalizedStrings.AddRangeAsync(
            strings, cancellationToken);

        await _dbContext.Tests.AddAsync(test, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}