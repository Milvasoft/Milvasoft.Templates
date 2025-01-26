using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

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
            .NotNullOrEmpty(localizer, MessageKey.GlobalName);
    }
}