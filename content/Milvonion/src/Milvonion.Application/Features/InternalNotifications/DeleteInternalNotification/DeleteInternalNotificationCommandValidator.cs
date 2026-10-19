using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.InternalNotifications.DeleteInternalNotification;

/// <summary>
/// Validations of <see cref="DeleteInternalNotificationCommand"/>.
/// </summary>
public sealed class DeleteInternalNotificationCommandValidator : AbstractValidator<DeleteInternalNotificationCommand>
{
    ///<inheritdoc cref="DeleteInternalNotificationCommandValidator"/>
    public DeleteInternalNotificationCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.InternalNotificationId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.InternalNotification]]);
    }
}