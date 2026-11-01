namespace DemyMath.Domain.Entities;

public class MultipleChoiceQuestion : Question
{
    public ICollection<QuestionOption> Options { get; set; }
        = new List<QuestionOption>();
}