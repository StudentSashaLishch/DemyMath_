namespace DemyMath.Application.Tests.Models;

public class TestResultDto
{
    public int CorrectAnswers { get; init; }
    public int TotalQuestions { get; init; }

    public IReadOnlyList<QuestionResultDto> Questions { get; init; } = [];
}

public class QuestionResultDto
{
    public int QuestionId { get; init; }
    public required string Text { get; init; }
    public bool IsCorrect { get; init; }
}