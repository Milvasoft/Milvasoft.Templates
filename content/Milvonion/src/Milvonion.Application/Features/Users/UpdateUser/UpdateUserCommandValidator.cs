using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Users.UpdateUser;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    ///<inheritdoc cref="UpdateUserCommandValidator"/>
    public UpdateUserCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.User]]);
    }
}