using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Roles.UpdateRole;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    ///<inheritdoc cref="UpdateRoleCommandValidator"/>
    public UpdateRoleCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Role]]);

        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .When(query => query.Name != null && query.Name.IsUpdated)
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateRoleCommand.Name)]]);
    }
}