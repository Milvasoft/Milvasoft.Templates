using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Account.DeleteNotifications;

/// <summary>
/// Validations of <see cref="DeleteNotificationsCommand"/>.
/// </summary>
public sealed class DeleteNotificationsCommandValidator : AbstractValidator<DeleteNotificationsCommand>
{
    ///<inheritdoc cref="DeleteNotificationsCommandValidator"/>
    public DeleteNotificationsCommandValidator(IMilvaLocalizer localizer)
    {
        RuleForEach(query => query.NotificationIdList)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.InternalNotification]]);
    }
}