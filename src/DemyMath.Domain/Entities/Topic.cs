namespace DemyMath.Domain.Entities;

public class Topic
{
    public int Id { get; set; }

    public required string TitleStringId { get; set; }
    public string? DescriptionStringId { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public string? Icon { get; set; }

    public ICollection<TopicPrerequisite> Prerequisites { get; set; }
        = new List<TopicPrerequisite>();

    public ICollection<ContentItem> ContentItems { get; set; }
        = new List<ContentItem>();
}