namespace DemyMath.Domain.Entities;

public class LocalizedString
{
    public required string StringId { get; set; }
    public required string Culture { get; set; }
    public required string Value { get; set; }

    public int VersionId { get; set; }
    public LearningContentVersion Version { get; set; } = null!;
}