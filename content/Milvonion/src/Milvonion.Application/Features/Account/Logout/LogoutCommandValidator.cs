using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Account.Logout;

/// <summary>
/// Validations of <see cref="LogoutCommand"/>.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    ///<inheritdoc cref="LogoutCommandValidator"/>
    public LogoutCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.UserName)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.IdentityInvalidUsername]);

        RuleFor(query => query.DeviceId)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.PossibleUIError]);
    }
}