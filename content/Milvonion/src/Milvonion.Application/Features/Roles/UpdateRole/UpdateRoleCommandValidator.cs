using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.Roles.UpdateRole;

/// <summary>
/// Validations of <see cref="UpdateRoleCommand"/>.
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
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(q => q.Name.IsUpdated);
    }
}