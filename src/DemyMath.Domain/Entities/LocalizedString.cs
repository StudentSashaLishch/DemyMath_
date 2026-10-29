namespace DemyMath.Domain.Entities;

public class LocalizedString
{
    public required string StringId { get; set; }
    public required string Culture { get; set; }
    public required string Value { get; set; }
}