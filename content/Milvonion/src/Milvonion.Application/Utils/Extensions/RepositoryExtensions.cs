namespace Milvonion.Application.Utils.Extensions;

/// <summary>
/// Contains extension methods for repositories.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Deletes every session of <paramref name="user"/> and archives the ones it carries (<see cref="User.Sessions"/>)
    /// to the session history. A user without loaded sessions (null or empty) only gets the delete.
    /// Call it inside the transaction that changes the user, and rotate the security stamp after the commit.
    /// </summary>
    /// <param name="userSessionRepository"></param>
    /// <param name="userSessionHistoriesRepository"></param>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task RevokeAllSessionsAsync(this IMilvonionRepositoryBase<UserSession> userSessionRepository,
                                                    IMilvonionRepositoryBase<UserSessionHistory> userSessionHistoriesRepository,
                                                    User user,
                                                    CancellationToken cancellationToken)
    {
        await userSessionRepository.ExecuteDeleteAsync(UserSession.Conditions.DeleteAllSessions(user.UserName), cancellationToken: cancellationToken);

        if (user.Sessions?.Count > 0)
            await userSessionHistoriesRepository.BulkAddAsync([.. user.Sessions.Select(s => new UserSessionHistory(s))], cancellationToken: cancellationToken);
    }
}
