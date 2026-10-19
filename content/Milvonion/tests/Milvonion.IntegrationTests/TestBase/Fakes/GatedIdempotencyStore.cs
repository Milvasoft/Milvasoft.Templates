using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Models;

namespace Milvonion.IntegrationTests.TestBase.Fakes;

/// <summary>
/// Wraps the real <see cref="IIdempotencyStore"/> and holds <see cref="CompleteAsync"/> until <see cref="Gate.Release"/>
/// is called, so a test can send a retry while the first attempt is still running (between its claim and its stored
/// response). Everything else is the real store.
/// </summary>
/// <param name="inner"></param>
/// <param name="gate"></param>
public sealed class GatedIdempotencyStore(IIdempotencyStore inner, GatedIdempotencyStore.Gate gate) : IIdempotencyStore
{
    private readonly IIdempotencyStore _inner = inner;
    private readonly Gate _gate = gate;

    /// <summary>
    /// The shared switch of a test: <see cref="Reached"/> completes when the first attempt waits to store its response,
    /// <see cref="Release"/> lets it continue.
    /// </summary>
    public sealed class Gate
    {
        private readonly TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Completes when an attempt is held before storing its response.</summary>
        public Task Reached => _reached.Task;

        /// <summary>Lets the held attempt store its response.</summary>
        public void Release() => _released.TrySetResult();

        internal Task WaitAsync()
        {
            _reached.TrySetResult();

            return _released.Task;
        }
    }

    /// <inheritdoc/>
    public Task<IdempotencyClaim> ClaimAsync(int userId, string key, string endpoint, byte[] requestHash, CancellationToken cancellationToken)
        => _inner.ClaimAsync(userId, key, endpoint, requestHash, cancellationToken);

    /// <inheritdoc/>
    public async Task CompleteAsync(int userId, string key, string responseData, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync();

        await _inner.CompleteAsync(userId, key, responseData, cancellationToken);
    }

    /// <inheritdoc/>
    public Task ReleaseAsync(int userId, string key, CancellationToken cancellationToken) => _inner.ReleaseAsync(userId, key, cancellationToken);
}
