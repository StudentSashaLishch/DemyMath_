namespace DemyMath.Application.Tests.Models;

public class TestDto
{
    public int Id { get; init; }
    public int TopicId { get; init; }
    public required string TopicTitle { get; init; }

    public IReadOnlyList<QuestionDto> Questions { get; init; } = [];
}

public class QuestionDto
{
    public int Id { get; init; }
    public required string Type { get; init; }
    public required string Text { get; init; }
    public int Order { get; init; }

    public IReadOnlyList<QuestionOptionDto> Options { get; init; } = [];

    public decimal? MinX { get; init; }
    public decimal? MaxX { get; init; }
    public decimal? MinY { get; init; }
    public decimal? MaxY { get; init; }
    public decimal? GridStep { get; init; }
}

public class QuestionOptionDto
{
    public int Id { get; init; }
    public required string Text { get; init; }
    public int Order { get; init; }
}