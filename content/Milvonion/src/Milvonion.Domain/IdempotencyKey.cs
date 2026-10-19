using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// One client-supplied idempotency key: "this user already sent this create request". A retry of the same
/// action (double tap, a request the network lost, the client's single retry after a token refresh) carries
/// the same <c>Idempotency-Key</c> header, hits the same row and gets the first attempt's response back
/// instead of creating a second record. Rows are short lived: <c>IdempotencyKeyCleanerJob</c> prunes them,
/// so the table stays small and hot.
/// </summary>
/// <remarks>
/// The primary key is <c>(UserId, Key)</c>: a key is only ever valid for the caller that minted it, so one
/// user can never consume another's. <see cref="RequestHash"/> pins the key to the request it was first used
/// with; the same key with a different body is rejected instead of silently replaying the wrong response.
/// </remarks>
[Table(TableNames.IdempotencyKeys)]
[Index(nameof(CreationDate))]
public class IdempotencyKey
{
    /// <summary>Caller the key belongs to (part of the primary key).</summary>
    public int UserId { get; set; }

    /// <summary>The key the client sent (part of the primary key).</summary>
    [MaxLength(64)]
    public string Key { get; set; }

    /// <summary>Route the key was first used on, for diagnostics.</summary>
    [MaxLength(120)]
    public string Endpoint { get; set; }

    /// <summary>SHA-256 of the route and the bound request, so the same key cannot be reused for another request.</summary>
    [MaxLength(32)]
    public byte[] RequestHash { get; set; }

    /// <summary>The first attempt's response envelope (JSON), replayed verbatim to retries. Null while the request is still running.</summary>
    public string ResponseData { get; set; }

    /// <summary>When the first attempt finished successfully. Null means "in progress" (or abandoned).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>When the key was claimed. Drives both the abandoned-claim takeover and the cleanup job.</summary>
    public DateTime CreationDate { get; set; }
}
