namespace Milvonion.Application.Interfaces;

/// <summary>
/// Server-side read of the kill switches (<see cref="FeatureFlag"/>) the management endpoints toggle. The switched off
/// keys are cached for a short window, so a guarded request does not cost a database round trip.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Whether the feature behind <paramref name="key"/> is enabled. Fails open: a missing row, an unknown key or
    /// a failed read all count as enabled.
    /// </summary>
    /// <param name="key">One of <see cref="FeatureFlagKeys"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>False only when an admin has switched the feature off.</returns>
    Task<bool> IsEnabledAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Drops the cached switch states, so a toggle takes effect on the next request instead of after the cache window.
    /// Call it after the change has been saved.
    /// </summary>
    /// <returns></returns>
    Task InvalidateAsync();
}
