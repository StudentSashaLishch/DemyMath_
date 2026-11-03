namespace DemyMath.Application.Tests.Models;

public class TestEditorDto
{
    public int TopicId { get; set; }
    public List<QuestionEditorDto> Questions { get; set; } = [];
}

public class QuestionEditorDto
{
    public int? Id { get; set; }
    public string Type { get; set; } = "MultipleChoice";
    public List<LocalizedValueDto> Translations { get; set; } = [];

    public List<QuestionOptionEditorDto> Options { get; set; } = [];

    public decimal CorrectAnswer { get; set; }
    public decimal Tolerance { get; set; }

    public decimal CorrectX { get; set; }
    public decimal CorrectY { get; set; }
    public decimal MinX { get; set; } = -5;
    public decimal MaxX { get; set; } = 5;
    public decimal MinY { get; set; } = -5;
    public decimal MaxY { get; set; } = 5;
    public decimal GridStep { get; set; } = 1;
}

public class QuestionOptionEditorDto
{
    public int? Id { get; set; }
    public bool IsCorrect { get; set; }
    public List<LocalizedValueDto> Translations { get; set; } = [];
}

public class LocalizedValueDto
{
    public string Culture { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}