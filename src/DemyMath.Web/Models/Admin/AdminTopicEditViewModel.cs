using DemyMath.Application.Topics.Models;

namespace DemyMath.Models.Admin;

public class AdminTopicEditViewModel
{
    public TopicEditorDto Topic { get; set; } = new();

    public IReadOnlyList<TopicDetailsDto> AvailableTopics { get; set; } = [];

    public IReadOnlyList<string> SupportedCultures { get; set; } = [];
}