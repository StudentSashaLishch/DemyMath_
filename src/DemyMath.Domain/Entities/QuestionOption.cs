namespace DemyMath.Domain.Entities;

public class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public MultipleChoiceQuestion Question { get; set; } = null!;

    public required string TextStringId { get; set; }

    public bool IsCorrect { get; set; }

    public int Order { get; set; }
}