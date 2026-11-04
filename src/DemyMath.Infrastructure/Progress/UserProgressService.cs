using DemyMath.Application.Progress;
using DemyMath.Application.Progress.Models;
using DemyMath.Domain.Entities;
using DemyMath.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemyMath.Infrastructure.Progress;

public class UserProgressService : IUserProgressService
{
    private readonly DemyMathDbContext _dbContext;

    public UserProgressService(DemyMathDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task MarkCompletedAsync(
        string userId,
        int topicId,
        CancellationToken cancellationToken = default)
    {
        var progressKey = await _dbContext.Topics
            .Where(t =>
                t.Id == topicId &&
                t.Version.Status == ContentVersionStatus.Published)
            .Select(t => (Guid?)t.ProgressKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (!progressKey.HasValue)
        {
            return;
        }

        await AddProgressAsync(
            userId,
            progressKey.Value,
            cancellationToken);
    }

    public async Task MarkCompletedIfNoTestAsync(
        string userId,
        int topicId,
        CancellationToken cancellationToken = default)
    {
        var topic = await _dbContext.Topics
            .Where(t =>
                t.Id == topicId &&
                t.Version.Status == ContentVersionStatus.Published)
            .Select(t => new
            {
                t.ProgressKey,
                HasTest = _dbContext.Tests.Any(test => test.TopicId == t.Id)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topic is null || topic.HasTest)
        {
            return;
        }

        await AddProgressAsync(
            userId,
            topic.ProgressKey,
            cancellationToken);
    }

    private async Task AddProgressAsync(
        string userId,
        Guid progressKey,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.UserTopicProgress
            .AnyAsync(
                p => p.UserId == userId &&
                     p.ProgressKey == progressKey,
                cancellationToken);

        if (exists)
        {
            return;
        }

        _dbContext.UserTopicProgress.Add(new UserTopicProgress
        {
            UserId = userId,
            ProgressKey = progressKey,
            CompletedAtUtc = DateTime.UtcNow
        });

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Захист від одночасного запису одного прогресу.
            var entry = _dbContext.ChangeTracker
                .Entries<UserTopicProgress>()
                .FirstOrDefault(e =>
                    e.Entity.UserId == userId &&
                    e.Entity.ProgressKey == progressKey);

            if (entry is not null)
            {
                entry.State = EntityState.Detached;
            }

            var alreadyExists = await _dbContext.UserTopicProgress
                .AnyAsync(
                    p => p.UserId == userId &&
                         p.ProgressKey == progressKey,
                    cancellationToken);

            if (!alreadyExists)
            {
                throw;
            }
        }
    }

    public async Task<HashSet<Guid>> GetCompletedProgressKeysAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from progress in _dbContext.UserTopicProgress.AsNoTracking()
            join topic in _dbContext.Topics.AsNoTracking()
                on progress.ProgressKey equals topic.ProgressKey
            where progress.UserId == userId &&
                  topic.Version.Status == ContentVersionStatus.Published
            select progress.ProgressKey
        ).ToHashSetAsync(cancellationToken);
    }

    public async Task<UserProgressSummaryDto> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var publishedKeys = _dbContext.Topics
            .Where(t => t.Version.Status == ContentVersionStatus.Published)
            .Select(t => t.ProgressKey);

        var totalCount = await publishedKeys
            .CountAsync(cancellationToken);

        var completedCount = await _dbContext.UserTopicProgress
            .Where(p =>
                p.UserId == userId &&
                publishedKeys.Contains(p.ProgressKey))
            .CountAsync(cancellationToken);

        return new UserProgressSummaryDto
        {
            TotalCount = totalCount,
            CompletedCount = completedCount
        };
    }

    public async Task ResetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.UserTopicProgress
            .Where(p => p.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}