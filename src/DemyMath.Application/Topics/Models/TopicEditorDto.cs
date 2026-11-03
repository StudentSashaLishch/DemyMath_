using DemyMath.Application.Localization.Models;

namespace DemyMath.Application.Topics.Models;

public class TopicEditorDto
{
    public int? Id { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public string? Icon { get; set; }

    public string TitleStringId { get; set; } = string.Empty;
    public string? DescriptionStringId { get; set; }

    public List<LocalizedValueDto> TitleTranslations { get; set; } = [];
    public List<LocalizedValueDto> DescriptionTranslations { get; set; } = [];

    public List<int> PrerequisiteTopicIds { get; set; } = [];
    public List<TopicContentItemEditDto> ContentItems { get; set; } = [];
}

public class TopicContentItemEditDto
{
    public int? Id { get; set; }
    public string Type { get; set; } = "Text";
    public string? VideoUrl { get; set; }
    public List<LocalizedValueDto> Translations { get; set; } = [];
}