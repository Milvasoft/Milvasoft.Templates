using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.DataAccess.EfCore.Utils;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Helpers;
using Milvonion.Domain;
using StackExchange.Redis;
using System.Text.Json;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Redis-backed revocation stamp store with the database (<c>User.SessionStamp</c>) as the durable
/// source of truth. Reads are served from Redis for a cheap per-request check; on a cache miss (or
/// any Redis failure) it falls back to the database and repopulates the cache, so a Redis outage
/// degrades to one extra database round trip rather than breaking authentication.
/// <para>
/// A rotation writes the new stamp through to Redis and must run after the caller's transaction has
/// committed. The read-through repopulation only adds a missing entry (SET NX), so a reader that loaded
/// the old stamp from the database just before a rotation can never put it back over the new one.
/// </para>
/// <para>
/// While Redis is disconnected the cache is skipped altogether: a command sent to a reconnecting multiplexer waits
/// in its backlog until the timeout, which would hold every authenticated request for seconds.
/// </para>
/// </summary>
/// <param name="cache"></param>
/// <param name="userRepository"></param>
/// <param name="cacheKeys"></param>
public class SecurityStampStore(IRedisAccessor cache, IMilvonionRepositoryBase<User> userRepository, CacheKeys cacheKeys) : ISecurityStampStore
{
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(12);

    private readonly IRedisAccessor _cache = cache;
    private readonly IMilvonionRepositoryBase<User> _userRepository = userRepository;

    // The key is CacheKeys.SecurityStamp under this application's prefix. Another service that shares this Redis and
    // these users (e.g. an admin API) builds the key with this application's CacheKeyOptions, so a revocation it
    // performs takes effect here at once.
    private readonly CacheKeys _cacheKeys = cacheKeys;

    /// <inheritdoc/>
    public async Task<string> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var current = await GetCurrentAsync(userId, cancellationToken);

        if (!string.IsNullOrEmpty(current))
            return current;

        // A user without a stamp yet (created by a seed or before the first login): assign and persist one now. There is
        // no old stamp that could be re-cached, and the caller (a login) may still be inside a transaction that rolls back,
        // so the entry is dropped rather than written through: the next read repopulates it from what the database committed.
        var stamp = await PersistAsync(userId, cancellationToken);

        await TryRemoveCacheAsync(userId);

        return stamp;
    }

    /// <inheritdoc/>
    public async Task<string> RotateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var stamp = await PersistAsync(userId, cancellationToken);

        // Write-through (the caller has committed): the shared entry holds the new stamp at once, and a reader that
        // still saw the old row cannot overwrite it because the read-through only adds a missing entry.
        await TrySetCacheAsync(userId, stamp);

        return stamp;
    }

    /// <inheritdoc/>
    public async Task<bool> IsValidAsync(int userId, string tokenStamp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(tokenStamp))
            return false;

        var current = await GetCurrentAsync(userId, cancellationToken);

        return !string.IsNullOrEmpty(current) && string.Equals(current, tokenStamp, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns the user's current stamp from Redis, falling back to the database (and repopulating the
    /// cache) on a miss or any Redis error. Never creates a stamp.
    /// </summary>
    private async Task<string> GetCurrentAsync(int userId, CancellationToken cancellationToken)
    {
        var cached = await TryGetCacheAsync(userId);

        if (!string.IsNullOrEmpty(cached))
            return cached;

        var stamp = await _userRepository.GetFirstOrDefaultAsync(u => u.Id == userId, projection: u => u.SessionStamp, cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(stamp))
            await TryAddCacheAsync(userId, stamp);

        return stamp;
    }

    /// <summary>
    /// Generates a new stamp and persists it to the user row.
    /// </summary>
    private async Task<string> PersistAsync(int userId, CancellationToken cancellationToken)
    {
        var stamp = Generate();

        await _userRepository.ExecuteUpdateAsync(userId, new SetPropertyBuilder<User>().SetPropertyValue(u => u.SessionStamp, stamp), cancellationToken);

        return stamp;
    }

    private async Task<string> TryGetCacheAsync(int userId)
    {
        if (!_cache.IsConnected())
            return null;

        try
        {
            // Stored via SetAsync (JSON-serialized), so read through the typed getter to get the plain value.
            return await _cache.GetAsync<string>(_cacheKeys.SecurityStamp(userId));
        }
        catch
        {
            return null;
        }
    }

    private async Task TrySetCacheAsync(int userId, string stamp)
    {
        if (!_cache.IsConnected())
            return;

        try
        {
            await _cache.SetAsync(_cacheKeys.SecurityStamp(userId), stamp, _cacheExpiry);
        }
        catch
        {
            // Redis unavailable: the database remains the source of truth; ignore.
        }
    }

    private async Task TryAddCacheAsync(int userId, string stamp)
    {
        if (!_cache.IsConnected())
            return;

        try
        {
            // Same JSON form as SetAsync writes (read back through the typed getter), but only when no entry exists:
            // a rotation that wrote the new stamp in the meantime always wins over this database read.
            await _cache.GetDatabase().StringSetAsync(_cacheKeys.SecurityStamp(userId), JsonSerializer.Serialize(stamp), _cacheExpiry, When.NotExists);
        }
        catch
        {
            // Redis unavailable: the database remains the source of truth; ignore.
        }
    }

    private async Task TryRemoveCacheAsync(int userId)
    {
        if (!_cache.IsConnected())
            return;

        try
        {
            await _cache.RemoveAsync(_cacheKeys.SecurityStamp(userId));
        }
        catch
        {
            // Redis unavailable: the database remains the source of truth; ignore.
        }
    }

    private static string Generate() => Guid.NewGuid().ToString("N");
}
