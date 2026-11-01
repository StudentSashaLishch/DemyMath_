namespace DemyMath.Domain.Entities;

public class Test
{
    public int Id { get; set; }

    public int TopicId { get; set; }
    public Topic Topic { get; set; } = null!;

    public ICollection<Question> Questions { get; set; }
        = new List<Question>();
}