using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.NotificationDtos;

namespace Milvonion.Application.Features.InternalNotifications.GetInternalNotificationDetail;

/// <summary>
/// Handles the internalNotification detail operation.
/// </summary>
/// <param name="internalnotificationRepository"></param>
public class GetInternalNotificationDetailQueryHandler(IMilvonionRepositoryBase<InternalNotification> internalnotificationRepository) : IInterceptable, IQueryHandler<GetInternalNotificationDetailQuery, InternalNotificationDetailDto>
{
    private readonly IMilvonionRepositoryBase<InternalNotification> _internalnotificationRepository = internalnotificationRepository;

    /// <inheritdoc/>
    public async Task<Response<InternalNotificationDetailDto>> Handle(GetInternalNotificationDetailQuery request, CancellationToken cancellationToken)
    {
        var internalNotification = await _internalnotificationRepository.GetByIdAsync(request.InternalNotificationId, projection: InternalNotificationDetailDto.Projection, cancellationToken: cancellationToken);

        if (internalNotification == null)
            return Response<InternalNotificationDetailDto>.Success(internalNotification, MessageKey.InternalNotificationNotFound, MessageType.Warning);

        return Response<InternalNotificationDetailDto>.Success(internalNotification);
    }
}
