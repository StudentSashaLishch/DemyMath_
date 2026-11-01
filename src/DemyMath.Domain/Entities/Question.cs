namespace DemyMath.Domain.Entities;

public abstract class Question
{
    public int Id { get; set; }

    public int TestId { get; set; }
    public Test Test { get; set; } = null!;

    public required string TextStringId { get; set; }

    public int Order { get; set; }
}