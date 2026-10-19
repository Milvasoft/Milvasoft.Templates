using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Account.Login;

/// <summary>
/// Validations of <see cref="LoginCommand"/>.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    ///<inheritdoc cref="LoginCommandValidator"/>
    public LoginCommandValidator(IMilvaLocalizer localizer)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Password)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(LoginCommand.Password)]]);

        RuleFor(query => query.DeviceId)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.PossibleUIError]);
    }
}