using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Users.DeleteUser;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    ///<inheritdoc cref="DeleteUserCommandValidator"/>
    public DeleteUserCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.UserId)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.User]]);
    }
}