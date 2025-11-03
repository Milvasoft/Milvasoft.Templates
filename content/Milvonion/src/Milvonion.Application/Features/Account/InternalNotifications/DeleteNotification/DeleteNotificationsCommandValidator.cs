using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.Account.InternalNotifications.DeleteNotification;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class DeleteNotificationsCommandValidator : AbstractValidator<DeleteNotificationsCommand>
{
    ///<inheritdoc cref="DeleteNotificationsCommandValidator"/>
    public DeleteNotificationsCommandValidator(IMilvaLocalizer localizer)
    {
        RuleForEach(query => query.NotificationIdList)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleForEach(query => query.NotificationIdList)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.InternalNotification]]);
    }
}