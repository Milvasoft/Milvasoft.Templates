using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;

namespace Milvonion.Application.Features.Users.DeleteUser;

/// <summary>
/// Handles the deletion of the user. The revocation stamp is rotated after the delete, so the deleted user's access
/// tokens are rejected at once instead of living on in the stamp cache.
/// </summary>
/// <param name="UserRepository"></param>
/// <param name="SecurityStampStore"></param>
[Log]
[UserActivityTrack(UserActivity.DeleteUser)]
public record DeleteUserCommandHandler(IMilvonionRepositoryBase<User> UserRepository,
                                       ISecurityStampStore SecurityStampStore) : IInterceptable, ICommandHandler<DeleteUserCommand, int>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = UserRepository;
    private readonly ISecurityStampStore _securityStampStore = SecurityStampStore;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetForDeleteAsync(request.UserId, cancellationToken: cancellationToken);

        if (user == null)
            return Response<int>.Error(0, MessageKey.UserNotFound);

        await _userRepository.DeleteAsync(user, cancellationToken: cancellationToken);

        await _securityStampStore.RotateAsync(request.UserId, cancellationToken);

        return Response<int>.Success(request.UserId);
    }
}
