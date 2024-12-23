using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.DeleteResourceGroup;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class DeleteResourceGroupCommandValidator : AbstractValidator<DeleteResourceGroupCommand>
{
    ///<inheritdoc cref="DeleteResourceGroupCommandValidator"/>
    public DeleteResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ResourceGroupId)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.ResourceGroupId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}