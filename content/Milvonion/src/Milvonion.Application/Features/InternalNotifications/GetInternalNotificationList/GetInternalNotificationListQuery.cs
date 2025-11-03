using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.InternalNotifications.GetInternalNotificationList;

/// <summary>
/// Data transfer object for internalNotification list.
/// </summary>
public record GetInternalNotificationListQuery : ListRequest, IListRequestQuery<AccountNotificationDto>
{
}