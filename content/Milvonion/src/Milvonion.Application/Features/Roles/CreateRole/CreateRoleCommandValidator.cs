using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Roles.CreateRole;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    ///<inheritdoc cref="CreateRoleCommandValidator"/>
    public CreateRoleCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(CreateRoleCommand.Name)]]);
    }
}