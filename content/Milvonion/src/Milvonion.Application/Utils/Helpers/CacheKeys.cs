using Milvonion.Application.Utils.Models;

namespace Milvonion.Application.Utils.Helpers;

/// <summary>
/// Builds every Redis key of the application, always as <c>{prefix}:{key}</c> with the prefix of
/// <see cref="CacheKeyOptions.Prefix"/>, so applications that share one Redis never read or overwrite each other's
/// entries (revocation stamps, rate limit counters, the feature flag cache). The <c>[Cache]</c> interceptor gets the same
/// prefix through its key configurator. A new Redis key is added here, never written as a literal at the call site.
/// </summary>
/// <remarks>
/// A service that must reach another application's keys on purpose (an admin API that revokes the tokens of the main
/// API's users) creates a second instance with that application's options instead of repeating the key format.
/// </remarks>
/// <param name="options"></param>
public sealed class CacheKeys(CacheKeyOptions options)
{
    private readonly CacheKeyOptions _options = options;

    /// <summary>
    /// The prefix every key of this instance starts with.
    /// </summary>
    public string Prefix => _options.Prefix;

    /// <summary>
    /// Key of a user's cached revocation stamp.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public string SecurityStamp(int userId) => Build($"sstamp:{userId}");

    /// <summary>
    /// Key of a rate limit counter: one bucket, one caller scope (user or address), one fixed time window.
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="scope"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    public string RateLimit(string bucket, string scope, long window) => Build($"rl:{bucket}:{scope}:{window}");

    /// <summary>
    /// Key of the cached list of the switched off feature flags.
    /// </summary>
    /// <returns></returns>
    public string DisabledFeatureFlags() => Build("feature-flags-disabled");

    /// <summary>
    /// Prefixes <paramref name="key"/> with the application's prefix.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string Build(string key) => $"{_options.Prefix}:{key}";
}
