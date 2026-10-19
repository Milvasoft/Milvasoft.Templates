using Mapster;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Identity.Abstract;
using Milvonion.Application.Utils.Helpers;

namespace Milvonion.Application.Features.Users.CreateUser;

/// <summary>
/// Handles the creation of the user.
/// </summary>
/// <param name="UserRepository"></param>
/// <param name="UserManager"></param>
[UserActivityTrack(UserActivity.CreateUser)]
public record CreateUserCommandHandler(IMilvonionRepositoryBase<User> UserRepository,
                                       IMilvaUserManager<User, int> UserManager) : IInterceptable, ICommandHandler<CreateUserCommand, int>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = UserRepository;
    private readonly IMilvaUserManager<User, int> _userManager = UserManager;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = request.Adapt<User>();

        var errorMessageForUserName = _userManager.ValidateUserName(user.UserName);

        if (errorMessageForUserName is not null)
            return Response<int>.Error(default, errorMessageForUserName);

        // The password policy is checked by CreateUserCommandValidator (IdentityPassword).
        _userManager.ConfigureForCreate(ref user, request.Password);

        _userRepository.FetchSoftDeletedEntities(true);

        var userWithSameUsername = await _userRepository.GetFirstOrDefaultAsync(i => i.NormalizedUserName == user.NormalizedUserName, projection: i => new User { Id = i.Id }, cancellationToken: cancellationToken);

        if (userWithSameUsername != null)
            return Response<int>.Error(0, MessageKey.UserExists);

        user.RoleRelations = request.RoleIdList?.Select(roleId => new UserRoleRelation { RoleId = roleId }).ToList();

        try
        {
            await _userRepository.AddAsync(user, cancellationToken);
        }
        catch (DbUpdateException ex) when (UserUniqueConstraintHelper.GetDuplicateMessageKey(ex) is string messageKey)
        {
            // Lost the race against a concurrent insert with the same username, or the e-mail is already taken.
            return Response<int>.Error(0, messageKey);
        }

        return Response<int>.Success(user.Id);
    }
}
