namespace DemyMath.Domain.Entities;

public enum ContentVersionStatus
{
    Draft,
    Published
}

public class LearningContentVersion
{
    public int Id { get; set; }

    public ContentVersionStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public ICollection<Topic> Topics { get; set; }
        = new List<Topic>();

    public ICollection<LocalizedString> LocalizedStrings { get; set; }
        = new List<LocalizedString>();
}