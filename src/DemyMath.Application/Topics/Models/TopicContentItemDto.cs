namespace DemyMath.Application.Topics.Models;

public class TopicContentItemDto
{
    public int Id { get; init; }
    public required string Type { get; init; }
    public int Order { get; init; }

    public string? Text { get; init; }
    public string? VideoUrl { get; init; }
}