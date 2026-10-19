using Microsoft.EntityFrameworkCore;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Persistence;

/// <summary>
/// Creates the <see cref="FeatureFlag"/> row of every key in <see cref="FeatureFlagKeys.All"/>, so each switch shows up
/// on the management endpoints (enabled) as soon as its key exists in code.
/// </summary>
public static class FeatureFlagSeeder
{
    /// <summary>
    /// Inserts the keys that have no row yet (enabled). Idempotent and never touches an existing row, so it is safe on
    /// every startup.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SeedAsync(MilvonionDbContext context, CancellationToken cancellationToken = default)
    {
        var existingKeys = (await context.FeatureFlags.Select(f => f.Key).ToListAsync(cancellationToken)).ToHashSet();

        var missingFlags = FeatureFlagKeys.All.Where(key => !existingKeys.Contains(key)).Select(key => new FeatureFlag
        {
            Key = key,
            IsEnabled = true,
        }).ToList();

        if (missingFlags.Count != 0)
        {
            await context.FeatureFlags.AddRangeAsync(missingFlags, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
