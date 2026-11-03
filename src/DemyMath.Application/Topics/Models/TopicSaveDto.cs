using DemyMath.Application.Localization.Models;

namespace DemyMath.Application.Topics.Models;

public class TopicSaveDto
{
    public int? Id { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public string? Icon { get; set; }

    public List<LocalizedValueDto> TitleTranslations { get; set; } = [];
    public List<LocalizedValueDto> DescriptionTranslations { get; set; } = [];

    public List<int> PrerequisiteTopicIds { get; set; } = [];
    public List<TopicContentItemSaveDto> ContentItems { get; set; } = [];
}

public class TopicContentItemSaveDto
{
    public int? Id { get; set; }
    public string Type { get; set; } = "Text";
    public string? VideoUrl { get; set; }
    public List<LocalizedValueDto> Translations { get; set; } = [];
}