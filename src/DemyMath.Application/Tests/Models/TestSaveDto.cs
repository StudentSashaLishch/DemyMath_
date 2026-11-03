namespace DemyMath.Application.Tests.Models;

public class TestSaveDto
{
    public int TopicId { get; set; }
    public List<QuestionSaveDto> Questions { get; set; } = [];
}

public class QuestionSaveDto
{
    public int? Id { get; set; }
    public string Type { get; set; } = "MultipleChoice";
    public List<LocalizedValueDto> Translations { get; set; } = [];

    public List<QuestionOptionSaveDto> Options { get; set; } = [];

    public decimal CorrectAnswer { get; set; }
    public decimal Tolerance { get; set; }

    public decimal CorrectX { get; set; }
    public decimal CorrectY { get; set; }
    public decimal MinX { get; set; }
    public decimal MaxX { get; set; }
    public decimal MinY { get; set; }
    public decimal MaxY { get; set; }
    public decimal GridStep { get; set; }
}

public class QuestionOptionSaveDto
{
    public int? Id { get; set; }
    public bool IsCorrect { get; set; }
    public List<LocalizedValueDto> Translations { get; set; } = [];
}