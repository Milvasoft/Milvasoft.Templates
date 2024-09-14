using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Roles.DeleteRole;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    ///<inheritdoc cref="DeleteRoleCommandValidator"/>
    public DeleteRoleCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.RoleId)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Role]]);
    }
}