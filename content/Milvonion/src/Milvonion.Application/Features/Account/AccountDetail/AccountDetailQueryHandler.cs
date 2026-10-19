using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.AccountDetail;

/// <summary>
/// Handles the query for retrieving the account details. The account is always the authenticated
/// caller's own: the id comes from the token, never from the request.
/// </summary>
/// <param name="userRepository"></param>
/// <param name="currentUser"></param>
public class AccountDetailQueryHandler(IMilvonionRepositoryBase<User> userRepository,
                                       ICurrentUser currentUser) : IInterceptable, IQueryHandler<AccountDetailQuery, AccountDetailDto>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = userRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    /// <inheritdoc/>
    public async Task<Response<AccountDetailDto>> Handle(AccountDetailQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUser.GetRequiredId();

        var user = await _userRepository.GetByIdAsync(currentUserId, projection: AccountDetailDto.Projection, cancellationToken: cancellationToken);

        if (user == null)
            return Response<AccountDetailDto>.Error(null, MessageKey.UserNotFound);

        return Response<AccountDetailDto>.Success(user);
    }
}
