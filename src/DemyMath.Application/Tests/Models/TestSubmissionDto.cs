namespace DemyMath.Application.Tests.Models;

public class TestSubmissionDto
{
    public List<QuestionAnswerSubmissionDto> Answers { get; set; } = [];
}

public class QuestionAnswerSubmissionDto
{
    public int QuestionId { get; set; }

    public int? SelectedOptionId { get; set; }

    public string? NumericAnswer { get; set; }

    public string? X { get; set; }
    public string? Y { get; set; }
}