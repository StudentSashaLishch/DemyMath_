
using DemyMath.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Data;

public sealed class DevelopmentDataSeeder
{
    private readonly DemyMathDbContext _dbContext;

    public DevelopmentDataSeeder(DemyMathDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        await _dbContext.ContentItems.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.TopicPrerequisites.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Topics.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.LocalizedStrings.ExecuteDeleteAsync(cancellationToken);

        var numbers = new Topic
        {
            TitleStringId = "topic.numbers.title",
            DescriptionStringId = "topic.numbers.description",
            PositionX = 120,
            PositionY = 120,
            Icon = "🔢"
        };

        var arithmeticTopic = new Topic
        {
            TitleStringId = "topic.arithmetic.title",
            DescriptionStringId = "topic.arithmetic.description",
            PositionX = 400,
            PositionY = 120,
            Icon = "➗"
        };

        var fractions = new Topic
        {
            TitleStringId = "topic.fractions.title",
            DescriptionStringId = "topic.fractions.description",
            PositionX = 680,
            PositionY = 120,
            Icon = "🥧"
        };

        arithmeticTopic.Prerequisites.Add(new TopicPrerequisite
        {
            Topic = arithmeticTopic,
            PrerequisiteTopic = numbers
        });

        fractions.Prerequisites.Add(new TopicPrerequisite
        {
            Topic = fractions,
            PrerequisiteTopic = arithmeticTopic
        });

        numbers.ContentItems.Add(new ContentItem
        {
            Type = ContentItemType.Text,
            Order = 1,
            TextStringId = "topic.numbers.content.intro"
        });

        arithmeticTopic.ContentItems.Add(new ContentItem
        {
            Type = ContentItemType.Text,
            Order = 1,
            TextStringId = "topic.arithmetic.content.intro"
        });

        arithmeticTopic.ContentItems.Add(new ContentItem
        {
            Type = ContentItemType.Video,
            Order = 2,
            VideoUrl = "https://www.youtube-nocookie.com/embed/aircAruvnKk"
        });

        fractions.ContentItems.Add(new ContentItem
        {
            Type = ContentItemType.Text,
            Order = 1,
            TextStringId = "topic.fractions.content.intro"
        });

        var initialTopics = new[]
        {
            numbers,
            arithmeticTopic,
            fractions
        };

        var strings = new[]
        {
            new LocalizedString
            {
                StringId = "topic.numbers.title",
                Culture = "uk",
                Value = "Числа"
            },
            new LocalizedString
            {
                StringId = "topic.numbers.title",
                Culture = "en",
                Value = "Numbers"
            },
            new LocalizedString
            {
                StringId = "topic.numbers.description",
                Culture = "uk",
                Value = "Основні поняття про числа."
            },
            new LocalizedString
            {
                StringId = "topic.numbers.description",
                Culture = "en",
                Value = "Basic concepts about numbers."
            },
            new LocalizedString
            {
                StringId = "topic.numbers.content.intro",
                Culture = "uk",
                Value = "Числа використовують для лічби, вимірювання та опису кількості."
            },
            new LocalizedString
            {
                StringId = "topic.numbers.content.intro",
                Culture = "en",
                Value = "Numbers are used for counting, measuring, and describing quantities."
            },

            new LocalizedString
            {
                StringId = "topic.arithmetic.title",
                Culture = "uk",
                Value = "Арифметичні дії"
            },
            new LocalizedString
            {
                StringId = "topic.arithmetic.title",
                Culture = "en",
                Value = "Arithmetic Operations"
            },
            new LocalizedString
            {
                StringId = "topic.arithmetic.description",
                Culture = "uk",
                Value = "Додавання, віднімання, множення та ділення."
            },
            new LocalizedString
            {
                StringId = "topic.arithmetic.description",
                Culture = "en",
                Value = "Addition, subtraction, multiplication, and division."
            },
            new LocalizedString
            {
                StringId = "topic.arithmetic.content.intro",
                Culture = "uk",
                Value = "Арифметичні дії дають змогу виконувати обчислення над числами."
            },
            new LocalizedString
            {
                StringId = "topic.arithmetic.content.intro",
                Culture = "en",
                Value = "Arithmetic operations allow calculations with numbers."
            },

            new LocalizedString
            {
                StringId = "topic.fractions.title",
                Culture = "uk",
                Value = "Дроби"
            },
            new LocalizedString
            {
                StringId = "topic.fractions.title",
                Culture = "en",
                Value = "Fractions"
            },
            new LocalizedString
            {
                StringId = "topic.fractions.description",
                Culture = "uk",
                Value = "Подання частин цілого."
            },
            new LocalizedString
            {
                StringId = "topic.fractions.description",
                Culture = "en",
                Value = "Representing parts of a whole."
            },
            new LocalizedString
            {
                StringId = "topic.fractions.content.intro",
                Culture = "uk",
                Value = "Дріб описує частину цілого та записується за допомогою чисельника і знаменника."
            },
            new LocalizedString
            {
                StringId = "topic.fractions.content.intro",
                Culture = "en",
                Value = "A fraction represents part of a whole and is written using a numerator and a denominator."
            }
        };

        await _dbContext.LocalizedStrings.AddRangeAsync(
            strings, cancellationToken);

        await _dbContext.Topics.AddRangeAsync(
            initialTopics, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
