using Microsoft.AspNetCore.Http;
using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.AccountDetail;

/// <summary>
/// Handles the query for retrieving the account details.
/// </summary>
public class AccountDetailQueryHandler(IMilvonionRepositoryBase<User> userRepository,
                                       IHttpContextAccessor httpContextAccessor) : IInterceptable, IQueryHandler<AccountDetailQuery, AccountDetailDto>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public async Task<Response<AccountDetailDto>> Handle(AccountDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, projection: AccountDetailDto.Projection, cancellationToken: cancellationToken);

        if (user == null)
            return Response<AccountDetailDto>.Success(default, MessageKey.UserNotFound, MessageType.Warning);

        if (!_httpContextAccessor.IsCurrentUser(user.UserName))
            return Response<AccountDetailDto>.Error(default, MessageKey.Unauthorized);

        return Response<AccountDetailDto>.Success(user);
    }
}
