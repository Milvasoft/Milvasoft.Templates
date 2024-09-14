using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.UserDtos;

namespace Milvonion.Application.Features.Users.GetUserList;

/// <summary>
/// Handles the user list operation.
/// </summary>
/// <param name="userRepository"></param>
public class GetUserListQueryHandler(IMilvonionRepositoryBase<User> userRepository) : IInterceptable, IListQueryHandler<GetUserListQuery, UserListDto>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = userRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<UserListDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var response = await _userRepository.GetAllAsync(request, null, UserListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}