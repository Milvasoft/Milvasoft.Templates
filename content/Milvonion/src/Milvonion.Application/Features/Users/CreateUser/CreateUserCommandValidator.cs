using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Users.CreateUser;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    ///<inheritdoc cref="CreateUserCommandValidator"/>
    public CreateUserCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.UserName)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(CreateUserCommand.UserName)]]);

        RuleFor(query => query.Password)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(CreateUserCommand.Password)]]);
    }
}