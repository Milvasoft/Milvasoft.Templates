using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

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
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Role]]);

        RuleFor(query => query.Name)
            .Must(name => name == null || (name.IsUpdated && !string.IsNullOrWhiteSpace(name.Value)))
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateRoleCommand.Name)]]);
    }
}