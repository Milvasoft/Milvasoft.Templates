using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.GetAccountNotifications;

/// <summary>
/// Handles the query for retrieving the caller's own notifications. The recipient comes from the token, never from the request.
/// </summary>
/// <param name="notificationRepository"></param>
/// <param name="currentUser"></param>
public class GetAccountNotificationsQueryHandler(IMilvonionRepositoryBase<InternalNotification> notificationRepository,
                                                 ICurrentUser currentUser) : IInterceptable, IListQueryHandler<GetAccountNotificationsQuery, AccountNotificationDto>
{
    private readonly IMilvonionRepositoryBase<InternalNotification> _notificationRepository = notificationRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    /// <inheritdoc/>
    public async Task<ListResponse<AccountNotificationDto>> Handle(GetAccountNotificationsQuery request, CancellationToken cancellationToken)
    {
        request.PageNumber ??= 1;
        request.RowCount = request.RowCount.ClampRowCount(20);

        var userName = _currentUser.UserName;

        if (string.IsNullOrEmpty(userName))
            return ListResponse<AccountNotificationDto>.Error(default, MessageKey.Unauthorized);

        var notifications = await _notificationRepository.GetAllAsync(request,
                                                                      condition: n => n.RecipientUserName == userName,
                                                                      projection: AccountNotificationDto.Projection,
                                                                      cancellationToken: cancellationToken);

        return notifications;
    }
}
