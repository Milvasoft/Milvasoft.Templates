namespace Milvonion.Application.Interfaces;

/// <summary>
/// Per-user revocation stamp store. The stamp is the source of truth for "are this user's tokens
/// still valid": it is baked into the access token at login and checked on every request.
/// Backed by Redis for a cheap per-request read, with the database (<c>User.SessionStamp</c>) as the
/// durable source of truth and a fail-safe fallback when Redis is unavailable.
/// </summary>
public interface ISecurityStampStore
{
    /// <summary>
    /// Returns the user's current stamp, creating and persisting one if the user has none yet.
    /// Used when minting a token so the token always carries a valid stamp.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates the user's stamp (new value persisted to the database and written through to the cache), instantly
    /// invalidating every access token that carried the old stamp. Called on password change, role change, lock
    /// and deletion. Call it after the transaction that revokes the sessions has committed (never inside it), so a
    /// concurrent request can never re-cache the old stamp.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> RotateAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a stamp presented by a token against the user's current stamp (Redis, with a
    /// database fallback). Returns true when they match.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tokenStamp"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsValidAsync(int userId, string tokenStamp, CancellationToken cancellationToken = default);
}
