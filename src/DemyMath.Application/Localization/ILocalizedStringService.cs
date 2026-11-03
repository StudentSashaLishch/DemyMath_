namespace DemyMath.Application.Localization;

public interface ILocalizedStringService
{
    Task<string> GetAsync(
        string stringId,
        string culture,
        int versionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetManyAsync(
        IEnumerable<string> stringIds,
        string culture,
        int versionId,
        CancellationToken cancellationToken = default);
}