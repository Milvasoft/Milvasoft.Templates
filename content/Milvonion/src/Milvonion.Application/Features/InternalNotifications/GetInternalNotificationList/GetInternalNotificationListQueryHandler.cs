using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.InternalNotifications.GetInternalNotificationList;

/// <summary>
/// Handles the internalNotification list operation.
/// </summary>
/// <param name="internalnotificationRepository"></param>
public class GetInternalNotificationListQueryHandler(IMilvonionRepositoryBase<InternalNotification> internalnotificationRepository) : IInterceptable, IListQueryHandler<GetInternalNotificationListQuery, AccountNotificationDto>
{
    private readonly IMilvonionRepositoryBase<InternalNotification> _internalnotificationRepository = internalnotificationRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<AccountNotificationDto>> Handle(GetInternalNotificationListQuery request, CancellationToken cancellationToken)
    {
        var response = await _internalnotificationRepository.GetAllAsync(request, projection: AccountNotificationDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
