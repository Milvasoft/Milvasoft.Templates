using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Milvonion.Application.Utils.Helpers;

/// <summary>
/// Maps a unique-index violation on <c>Users</c> to the message key the pre-insert checks would have returned.
/// The check-then-insert in CreateUser is racy by nature: two requests can pass the "is this username taken?"
/// query at the same time and only the database can reject the second one (23505). Without this, that loser gets
/// a generic "duplicate data" answer instead of "this username is already taken".
/// </summary>
public static class UserUniqueConstraintHelper
{
    // Unique indexes on Users (see the User entity attributes and MilvonionDbContext.OnModelCreating) and the message they stand for.
    private static readonly Dictionary<string, string> _messageKeyByIndex = new(StringComparer.Ordinal)
    {
        ["IX_Users_NormalizedUserName"] = MessageKey.UserExists,
        ["IX_Users_UserName_IsDeleted_DeletionDate"] = MessageKey.UserExists,
        ["IX_Users_NormalizedEmail"] = MessageKey.EmailExists,
    };

    /// <summary>
    /// Returns the message key for a duplicate username / e-mail violation carried by
    /// <paramref name="exception"/>, or null when the exception is something else (the caller rethrows).
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static string GetDuplicateMessageKey(DbUpdateException exception)
    {
        if (exception?.InnerException is not PostgresException postgres || postgres.SqlState != MessageConstant.DuplicateDataViolationPgCode)
            return null;

        return !string.IsNullOrEmpty(postgres.ConstraintName) && _messageKeyByIndex.TryGetValue(postgres.ConstraintName, out var messageKey) ? messageKey : null;
    }
}
