namespace DemyMath.Domain.Entities;

public class UserTopicProgress
{
    public required string UserId { get; set; }
    public Guid ProgressKey { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}