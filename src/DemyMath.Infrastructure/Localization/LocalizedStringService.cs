using DemyMath.Application.Localization;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Localization;

public class LocalizedStringService : ILocalizedStringService
{
    private readonly DemyMathDbContext _dbContext;

    public LocalizedStringService(DemyMathDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetAsync(
        string stringId,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var value = await _dbContext.LocalizedStrings
            .Where(s => s.StringId == stringId &&
                        s.Culture == culture)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return value ?? stringId;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetManyAsync(
        IEnumerable<string> stringIds,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var ids = stringIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<string, string>();
        }

        var values = await _dbContext.LocalizedStrings
            .Where(s => ids.Contains(s.StringId) &&
                        s.Culture == culture)
            .ToDictionaryAsync(
                s => s.StringId,
                s => s.Value,
                cancellationToken);

        foreach (var id in ids)
        {
            values.TryAdd(id, id);
        }

        return values;
    }
}