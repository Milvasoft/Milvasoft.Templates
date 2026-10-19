namespace Milvonion.Application.Utils.Models;

/// <summary>
/// Outcome of claiming an idempotency key: either this request owns the key and may run, or the key already
/// exists and the caller is a retry.
/// </summary>
public class IdempotencyClaim
{
    /// <summary>True when this request claimed the key and must do the work.</summary>
    public bool Claimed { get; set; }

    /// <summary>True when the stored key was first used with a different request (same key, other body).</summary>
    public bool RequestMismatch { get; set; }

    /// <summary>The first attempt's stored response envelope (JSON) when it already finished; null otherwise.</summary>
    public string ResponseData { get; set; }
}
