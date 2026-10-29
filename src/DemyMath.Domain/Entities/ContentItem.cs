namespace DemyMath.Domain.Entities;

public enum ContentItemType
{
    Text,
    Video
}

public class ContentItem
{
    public int Id { get; set; }

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;

    public ContentItemType Type { get; set; }
    public int Order { get; set; }

    public string? TextStringId { get; set; }
    public string? VideoUrl { get; set; }
}