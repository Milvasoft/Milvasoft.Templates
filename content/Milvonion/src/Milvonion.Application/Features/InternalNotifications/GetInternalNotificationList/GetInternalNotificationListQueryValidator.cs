using FluentValidation;

namespace Milvonion.Application.Features.InternalNotifications.GetInternalNotificationList;

/// <summary>
/// Validations of <see cref="GetInternalNotificationListQuery"/>.
/// </summary>
public sealed class GetInternalNotificationListQueryValidator : AbstractValidator<GetInternalNotificationListQuery>
{
    ///<inheritdoc cref="GetInternalNotificationListQueryValidator"/>
    public GetInternalNotificationListQueryValidator()
    {
    }
}