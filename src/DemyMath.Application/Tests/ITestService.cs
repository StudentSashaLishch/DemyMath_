using DemyMath.Application.Tests.Models;

namespace DemyMath.Application.Tests;

public interface ITestService
{
    Task<TestDto?> GetByTopicIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default);

    Task<TestResultDto?> SubmitAsync(
        int topicId,
        TestSubmissionDto submission,
        string culture,
        CancellationToken cancellationToken = default);
}