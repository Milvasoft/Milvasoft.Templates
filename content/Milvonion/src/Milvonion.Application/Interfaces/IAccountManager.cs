using Milvasoft.Core.Abstractions;
using Milvasoft.Identity.Concrete;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Account manager for user account operations.
/// </summary>
public interface IAccountManager : IInterceptable
{
    /// <summary>
    /// User login operation.
    /// It updates sessions in database.
    /// Generates access and refresh tokens.
    /// </summary>
    /// <returns></returns>
    Task<MilvaToken> LoginAsync(User user, string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates token for ci user.
    /// Generates access token carrying the user's current revocation stamp.
    /// </summary>
    /// <returns></returns>
    Task<string> GenerateTokenAsync(User user, DateTime? expiryDate = null, CancellationToken cancellationToken = default);
}
