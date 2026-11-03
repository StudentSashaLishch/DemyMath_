using DemyMath.Domain.Entities;
using DemyMath.Application.Tests.Models;

namespace DemyMath.Application.Tests;

public interface ITestService
{
    Task<TestDto?> GetByTopicIdAsync(
        int topicId,
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published);

    Task<TestResultDto?> SubmitAsync(
        int topicId,
        TestSubmissionDto submission,
        string culture,
        CancellationToken cancellationToken = default,
        ContentVersionStatus versionStatus = ContentVersionStatus.Published);
}