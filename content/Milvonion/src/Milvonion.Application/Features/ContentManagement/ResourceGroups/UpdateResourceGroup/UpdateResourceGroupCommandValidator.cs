using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.UpdateResourceGroup;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class UpdateResourceGroupCommandValidator : AbstractValidator<UpdateResourceGroupCommand>
{
    ///<inheritdoc cref="UpdateResourceGroupCommandValidator"/>
    public UpdateResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.ResourceGroup]]);

        RuleFor(query => query.Name)
            .Must(name => name == null || (name.IsUpdated && !string.IsNullOrWhiteSpace(name.Value)))
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateResourceGroupCommand.Name)]]);
    }
}