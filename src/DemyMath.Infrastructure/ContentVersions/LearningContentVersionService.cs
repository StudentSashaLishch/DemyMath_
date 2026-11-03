public async Task PublishDraftAsync(
    CancellationToken cancellationToken = default)
{
    await using var transaction =
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);

    var draft = await _dbContext.LearningContentVersions
        .FirstOrDefaultAsync(
            v => v.Status == ContentVersionStatus.Draft,
            cancellationToken);

    if (draft is null)
    {
        throw new InvalidOperationException(
            "No draft version exists.");
    }

    var published = await _dbContext.LearningContentVersions
        .FirstOrDefaultAsync(
            v => v.Status == ContentVersionStatus.Published,
            cancellationToken);

    if (published is not null)
    {
        _dbContext.LearningContentVersions.Remove(published);

        // Спочатку видаляємо попередню опубліковану версію,
        // щоб не порушити унікальний індекс Published.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    draft.Status = ContentVersionStatus.Published;
    draft.PublishedAtUtc = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}