using DemyMath.Application.Tests.Models;

namespace DemyMath.Application.Tests;

public interface ITestManagementService
{
    Task<TestEditorDto?> GetForEditAsync(
        int topicId,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        TestSaveDto dto,
        IReadOnlyCollection<string> supportedCultures,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int topicId,
        CancellationToken cancellationToken = default);
}