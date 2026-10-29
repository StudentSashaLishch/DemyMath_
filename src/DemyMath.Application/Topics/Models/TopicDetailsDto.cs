namespace DemyMath.Application.Topics.Models;

public class TopicDetailsDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }

    public double PositionX { get; init; }
    public double PositionY { get; init; }
    public string? Icon { get; init; }

    public IReadOnlyList<int> PrerequisiteTopicIds { get; init; }
        = [];

    public IReadOnlyList<TopicContentItemDto> ContentItems { get; init; }
        = [];
}