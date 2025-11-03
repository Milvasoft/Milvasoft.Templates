using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.NotificationDtos;

namespace Milvonion.Application.Features.InternalNotifications.GetInternalNotificationDetail;

/// <summary>
/// Data transfer object for internalNotification details.
/// </summary>
public record GetInternalNotificationDetailQuery : IQuery<InternalNotificationDetailDto>
{
    /// <summary>
    /// InternalNotification id to access details.
    /// </summary>
    public long InternalNotificationId { get; set; }
}
