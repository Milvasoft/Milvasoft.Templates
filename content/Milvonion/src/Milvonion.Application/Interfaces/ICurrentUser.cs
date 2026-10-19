namespace Milvonion.Application.Interfaces;

/// <summary>
/// Ambient accessor for the authenticated caller, resolved from the request's already-validated
/// JWT claims (never from the database and never by re-parsing the token). Replaces the
/// "read the user name from the token, then SELECT the user to get its id" round trip: the id is
/// carried in the token, so handlers read it straight from here.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The current user's id from the token, or null when the request is anonymous.
    /// </summary>
    int? Id { get; }

    /// <summary>
    /// The current user's user name from the token, or null when anonymous.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// The current user's type from the token, or null when anonymous / not present.
    /// </summary>
    UserType? UserType { get; }

    /// <summary>
    /// Whether the request carries an authenticated identity.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The current user's id, throwing an unauthorized (401) response when it is not present.
    /// </summary>
    int GetRequiredId();
}
