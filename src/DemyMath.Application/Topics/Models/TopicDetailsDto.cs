namespace DemyMath.Application.Topics.Models;

public class TopicDetailsDto
{
    public int Id { get; init; }
    public Guid ProgressKey { get; init; }
    public bool IsCompleted { get; set; }
    public required string Title { get; init; }
    public string? Description { get; init; }

    public double PositionX { get; init; }
    public double PositionY { get; init; }
    public string? Icon { get; init; }

    public bool HasTest { get; init; }

    public IReadOnlyList<PrerequisiteTopicDto> Prerequisites { get; set; }
        = [];

    public IReadOnlyList<TopicContentItemDto> ContentItems { get; init; }
        = [];
}