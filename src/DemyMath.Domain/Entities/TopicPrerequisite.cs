namespace DemyMath.Domain.Entities;

public class TopicPrerequisite
{
    public int TopicId { get; set; }
    public int PrerequisiteTopicId { get; set; }

    public Topic Topic { get; set; } = null!;
    public Topic PrerequisiteTopic { get; set; } = null!;
}