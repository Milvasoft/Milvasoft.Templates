using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Helpers;
using Milvonion.Domain;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Default <see cref="IFeatureFlagService"/>. The keys an admin switched off are read from the <c>FeatureFlags</c> table
/// and cached in Redis for <see cref="CacheExpiry"/>, so a guarded request costs one cache read; a toggle drops the
/// entry (<see cref="InvalidateAsync"/>) and applies on the next request.
/// </summary>
/// <remarks>
/// While Redis is disconnected the cache is skipped and every read goes to the database; any read failure counts as
/// enabled (fail-open), so a flag problem never takes a feature down on its own.
/// </remarks>
/// <param name="cache"></param>
/// <param name="featureFlagRepository"></param>
/// <param name="logger"></param>
/// <param name="cacheKeys"></param>
public class FeatureFlagService(IRedisAccessor cache, IMilvonionRepositoryBase<FeatureFlag> featureFlagRepository, IMilvaLogger logger, CacheKeys cacheKeys) : IFeatureFlagService
{
    /// <summary>How long the switched off keys are served from the cache.</summary>
    public static readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(60);

    private readonly IRedisAccessor _cache = cache;
    private readonly IMilvonionRepositoryBase<FeatureFlag> _featureFlagRepository = featureFlagRepository;
    private readonly IMilvaLogger _logger = logger;
    private readonly CacheKeys _cacheKeys = cacheKeys;

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var disabledKeys = await GetDisabledKeysAsync(cancellationToken);

            return !disabledKeys.Contains(key);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fail open: a flag read problem must never take a feature down on its own.
            _logger?.Warning(ex, "Feature flag {FlagKey} could not be read; treating it as enabled.", key);

            return true;
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateAsync()
    {
        if (!_cache.IsConnected())
            return;

        try
        {
            await _cache.RemoveAsync(_cacheKeys.DisabledFeatureFlags());
        }
        catch
        {
            // Redis unavailable: nothing is served from it either; ignore.
        }
    }

    /// <summary>
    /// Returns the switched off keys from the cache, falling back to the database (and repopulating the cache) on a miss.
    /// </summary>
    private async Task<List<string>> GetDisabledKeysAsync(CancellationToken cancellationToken)
    {
        var cached = await TryGetCacheAsync();

        if (cached != null)
            return cached;

        var disabledKeys = await _featureFlagRepository.GetAllAsync(f => !f.IsEnabled, projection: f => f.Key, cancellationToken: cancellationToken);

        await TrySetCacheAsync(disabledKeys);

        return disabledKeys;
    }

    private async Task<List<string>> TryGetCacheAsync()
    {
        if (!_cache.IsConnected())
            return null;

        try
        {
            return await _cache.GetAsync<List<string>>(_cacheKeys.DisabledFeatureFlags());
        }
        catch
        {
            return null;
        }
    }

    private async Task TrySetCacheAsync(List<string> disabledKeys)
    {
        if (!_cache.IsConnected())
            return;

        try
        {
            await _cache.SetAsync(_cacheKeys.DisabledFeatureFlags(), disabledKeys, CacheExpiry);
        }
        catch
        {
            // Redis unavailable: the database remains the source of truth; ignore.
        }
    }
}
