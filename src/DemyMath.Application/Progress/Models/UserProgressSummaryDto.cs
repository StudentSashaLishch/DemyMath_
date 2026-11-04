namespace DemyMath.Application.Progress.Models;

public class UserProgressSummaryDto
{
    public int CompletedCount { get; init; }
    public int TotalCount { get; init; }

    public int Percentage =>
        TotalCount == 0
            ? 0
            : (int)Math.Round(CompletedCount * 100.0 / TotalCount);
}