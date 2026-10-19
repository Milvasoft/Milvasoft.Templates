using Microsoft.AspNetCore.Http;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;

namespace Milvonion.Application.Features.Account.ChangePassword;

/// <summary>
/// Handles the change password command. The password and the session revocation commit together
/// (<see cref="ITransactionRunner"/>); the revocation stamp is rotated after the commit, so a concurrent request cannot
/// put the old stamp back into the cache.
/// </summary>
/// <param name="UserRepository"></param>
/// <param name="UserSessionRepository"></param>
/// <param name="UserSessionHistoriesRepository"></param>
/// <param name="MilvaUserManager"></param>
/// <param name="SecurityStampStore"></param>
/// <param name="HttpContextAccessor"></param>
/// <param name="TransactionRunner"></param>
public record ChangePasswordCommandHandler(IMilvonionRepositoryBase<User> UserRepository,
                                           IMilvonionRepositoryBase<UserSession> UserSessionRepository,
                                           IMilvonionRepositoryBase<UserSessionHistory> UserSessionHistoriesRepository,
                                           IMilvaUserManager<User, int> MilvaUserManager,
                                           ISecurityStampStore SecurityStampStore,
                                           IHttpContextAccessor HttpContextAccessor,
                                           ITransactionRunner TransactionRunner) : IInterceptable, ICommandHandler<ChangePasswordCommand>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = UserRepository;
    private readonly IMilvaUserManager<User, int> _milvaUserManager = MilvaUserManager;
    private readonly ISecurityStampStore _securityStampStore = SecurityStampStore;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContextAccessor;
    private readonly IMilvonionRepositoryBase<UserSession> _userSessionRepository = UserSessionRepository;
    private readonly IMilvonionRepositoryBase<UserSessionHistory> _userSessionHistoriesRepository = UserSessionHistoriesRepository;
    private readonly ITransactionRunner _tx = TransactionRunner;

    /// <inheritdoc/>
    public async Task<Response> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(i => i.UserName == request.UserName, User.Projections.ChangePassword, cancellationToken: cancellationToken);

        if (user == null || !_httpContextAccessor.IsCurrentUser(user.UserName))
            return Response<MilvaToken>.Error(null, MessageKey.Unauthorized);

        if (!_milvaUserManager.CheckPassword(user, request.OldPassword))
            return Response<MilvaToken>.Error(null, MessageKey.WrongPassword);

        // The password policy is checked by ChangePasswordCommandValidator (IdentityPassword).
        _milvaUserManager.SetPasswordHash(user, request.NewPassword);

        await _tx.RunAsync(async () =>
        {
            await _userRepository.UpdateAsync(user, cancellationToken, u => u.PasswordHash);

            await _userSessionRepository.RevokeAllSessionsAsync(_userSessionHistoriesRepository, user, cancellationToken);
        });

        // Rotate the revocation stamp (after the commit) so any access token still in flight is rejected immediately.
        await _securityStampStore.RotateAsync(user.Id, cancellationToken);

        return Response.Success();
    }
}
