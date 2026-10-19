using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.CreateResourceGroup;

/// <summary>
/// Validations of <see cref="CreateResourceGroupCommand"/>.
/// </summary>
public sealed class CreateResourceGroupCommandValidator : AbstractValidator<CreateResourceGroupCommand>
{
    ///<inheritdoc cref="CreateResourceGroupCommandValidator"/>
    public CreateResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName);
    }
}