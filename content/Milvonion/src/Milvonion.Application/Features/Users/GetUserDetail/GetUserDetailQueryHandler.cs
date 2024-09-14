using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.UserDtos;

namespace Milvonion.Application.Features.Users.GetUserDetail;

/// <summary>
/// Handles the user detail operation.
/// </summary>
/// <param name="userRepository"></param>
public class GetUserDetailQueryHandler(IMilvonionRepositoryBase<User> userRepository) : IInterceptable, IQueryHandler<GetUserDetailQuery, UserDetailDto>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = userRepository;

    /// <inheritdoc/>
    public async Task<Response<UserDetailDto>> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, null, UserDetailDto.Projection, cancellationToken: cancellationToken);

        if (user == null)
            return Response<UserDetailDto>.Success(user, MessageKey.UserNotFound, MessageType.Warning);

        return Response<UserDetailDto>.Success(user);
    }
}
