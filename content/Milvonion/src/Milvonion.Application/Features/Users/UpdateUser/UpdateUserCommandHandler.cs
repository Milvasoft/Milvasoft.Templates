using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete.Options;

namespace Milvonion.Application.Features.Users.UpdateUser;

/// <summary>
/// Handles the update of the user. The changes and the session revocation commit together
/// (<see cref="ITransactionRunner"/>); the revocation stamp is rotated after the commit, so a concurrent request cannot
/// put the old stamp back into the cache.
/// </summary>
/// <param name="UserRepository"></param>
/// <param name="UserRoleRelationRepository"></param>
/// <param name="UserSessionRepository"></param>
/// <param name="UserSessionHistoriesRepository"></param>
/// <param name="SecurityStampStore"></param>
/// <param name="TransactionRunner"></param>
/// <param name="MilvaIdentityOptions"></param>
/// <param name="MilvaPasswordHasher"></param>
[UserActivityTrack(UserActivity.UpdateUser)]
public record UpdateUserCommandHandler(IMilvonionRepositoryBase<User> UserRepository,
                                       IMilvonionRepositoryBase<UserRoleRelation> UserRoleRelationRepository,
                                       IMilvonionRepositoryBase<UserSession> UserSessionRepository,
                                       IMilvonionRepositoryBase<UserSessionHistory> UserSessionHistoriesRepository,
                                       ISecurityStampStore SecurityStampStore,
                                       ITransactionRunner TransactionRunner,
                                       Lazy<MilvaIdentityOptions> MilvaIdentityOptions,
                                       Lazy<IMilvaPasswordHasher> MilvaPasswordHasher) : IInterceptable, ICommandHandler<UpdateUserCommand, int>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = UserRepository;
    private readonly IMilvonionRepositoryBase<UserRoleRelation> _userRoleRelationRepository = UserRoleRelationRepository;
    private readonly IMilvonionRepositoryBase<UserSession> _userSessionRepository = UserSessionRepository;
    private readonly IMilvonionRepositoryBase<UserSessionHistory> _userSessionHistoriesRepository = UserSessionHistoriesRepository;
    private readonly ISecurityStampStore _securityStampStore = SecurityStampStore;
    private readonly ITransactionRunner _tx = TransactionRunner;
    private readonly Lazy<MilvaIdentityOptions> _milvaIdentityOptions = MilvaIdentityOptions;
    private readonly Lazy<IMilvaPasswordHasher> _milvaPasswordHasher = MilvaPasswordHasher;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var setPropertyBuilder = _userRepository.GetUpdatablePropertiesBuilder(request);

        if (request.Lockout.IsUpdated)
        {
            if (request.Lockout.Value)
                setPropertyBuilder.SetPropertyValue(u => u.LockoutEnd, DateTimeOffset.UtcNow.AddMinutes(_milvaIdentityOptions.Value.Lockout.DefaultLockoutMinute));
            else
                setPropertyBuilder.SetPropertyValue(u => u.LockoutEnd, null).SetPropertyValue(u => u.AccessFailedCount, 0);
        }

        // The password policy is checked by UpdateUserCommandValidator (IdentityPassword).
        if (request.NewPassword.IsUpdated)
        {
            var hashedPassword = _milvaPasswordHasher.Value.HashPassword(request.NewPassword.Value);

            setPropertyBuilder.SetPropertyValue(u => u.PasswordHash, hashedPassword);
        }

        // Security-relevant admin changes (lock, password reset, role change) revoke the user's sessions and rotate the
        // revocation stamp, so both the refresh token and any in-flight access token die.
        var shouldRevoke = (request.Lockout.IsUpdated && request.Lockout.Value)
                           || request.NewPassword.IsUpdated
                           || request.RoleIdList.IsUpdated;

        var updatedCount = 0;

        await _tx.RunAsync(async () =>
        {
            // Modification auditing (appsettings) sets the audit columns on every update, so no affected row means there is no such user.
            updatedCount = await _userRepository.ExecuteUpdateAsync(request.Id, setPropertyBuilder, cancellationToken: cancellationToken);

            if (updatedCount == 0)
                return;

            if (request.RoleIdList.IsUpdated)
            {
                await _userRoleRelationRepository.ExecuteDeleteAsync(rl => rl.UserId == request.Id, cancellationToken: cancellationToken);

                var addedEntities = request.RoleIdList.Value?.Distinct()
                                                             .Select(roleId => new UserRoleRelation { RoleId = roleId, UserId = request.Id })
                                                             .ToList();

                if (addedEntities?.Count > 0)
                    await _userRoleRelationRepository.BulkAddAsync(addedEntities, null, cancellationToken);
            }

            if (shouldRevoke)
            {
                var user = await _userRepository.GetByIdAsync(request.Id, projection: User.Projections.UpdateUserWithSessions, cancellationToken: cancellationToken);

                // A missing user has no sessions to revoke.
                if (user != null)
                    await _userSessionRepository.RevokeAllSessionsAsync(_userSessionHistoriesRepository, user, cancellationToken);
            }
        });

        if (updatedCount == 0)
            return Response<int>.Error(0, MessageKey.UserNotFound);

        if (shouldRevoke)
            await _securityStampStore.RotateAsync(request.Id, cancellationToken);

        return Response<int>.Success(request.Id);
    }
}
