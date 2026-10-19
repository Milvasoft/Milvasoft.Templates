using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Account.MarkNotificationsAsSeen;

/// <summary>
/// Validations of <see cref="MarkNotificationsAsSeenCommand"/>.
/// </summary>
public sealed class MarkNotificationsAsSeenCommandValidator : AbstractValidator<MarkNotificationsAsSeenCommand>
{
    ///<inheritdoc cref="MarkNotificationsAsSeenCommandValidator"/>
    public MarkNotificationsAsSeenCommandValidator(IMilvaLocalizer localizer)
    {
        RuleForEach(query => query.NotificationIdList)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.InternalNotification]]);
    }
}