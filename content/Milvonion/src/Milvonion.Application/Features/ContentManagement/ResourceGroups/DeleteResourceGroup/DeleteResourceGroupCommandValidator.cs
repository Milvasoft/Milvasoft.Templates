using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.DeleteResourceGroup;

/// <summary>
/// Validations of <see cref="DeleteResourceGroupCommand"/>.
/// </summary>
public sealed class DeleteResourceGroupCommandValidator : AbstractValidator<DeleteResourceGroupCommand>
{
    ///<inheritdoc cref="DeleteResourceGroupCommandValidator"/>
    public DeleteResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ResourceGroupId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}