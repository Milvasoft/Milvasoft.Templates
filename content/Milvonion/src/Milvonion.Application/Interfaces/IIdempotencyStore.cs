using Milvonion.Application.Utils.Models;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Stores the client-supplied idempotency keys of create endpoints so a retried request returns the first
/// attempt's response instead of creating a second row. Backed by a table (not the cache) because losing a
/// key means a duplicate: the window must survive a restart and an eviction.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Claims <paramref name="key"/> for <paramref name="userId"/> in a single statement. Returns a claim when
    /// this request owns the key (nothing stored yet, or the previous claim was abandoned), otherwise the state
    /// of the stored key: its response when the first attempt finished, or neither when it is still running.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="key"></param>
    /// <param name="endpoint"></param>
    /// <param name="requestHash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IdempotencyClaim> ClaimAsync(int userId, string key, string endpoint, byte[] requestHash, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the successful response of a claimed key, so every later retry replays it.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="key"></param>
    /// <param name="responseData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteAsync(int userId, string key, string responseData, CancellationToken cancellationToken);

    /// <summary>
    /// Releases a claimed key whose request failed, so the caller can retry the action right away.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReleaseAsync(int userId, string key, CancellationToken cancellationToken);
}
