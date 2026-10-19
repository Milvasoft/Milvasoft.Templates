namespace Milvonion.IntegrationTests.TestBase;

/// <summary>
/// Base of every integration test class: shares the collection's <see cref="MilvonionApiFactory"/> and flushes Redis
/// after each test.
/// </summary>
/// <param name="factory"></param>
public abstract class IntegrationTestBase(MilvonionApiFactory factory) : IAsyncLifetime
{
    /// <summary>The shared factory (one host, one database for the whole run).</summary>
    protected readonly MilvonionApiFactory _factory = factory;

    /// <inheritdoc/>
    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public virtual async ValueTask DisposeAsync()
    {
        // The database is shared and never reset, but the cached revocation stamps and feature flags and the rate limit
        // counters are flushed, so a test never sees another test's cache entry or rate limit window.
        await _factory.FlushRedisAsync();

        GC.SuppressFinalize(this);
    }
}
