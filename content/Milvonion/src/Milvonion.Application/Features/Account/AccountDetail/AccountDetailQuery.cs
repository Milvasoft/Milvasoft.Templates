using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.AccountDetail;

/// <summary>
/// Data transfer object for account details. The account is always the authenticated caller's own (taken from the token).
/// </summary>
public record AccountDetailQuery : IQuery<AccountDetailDto>
{
}
