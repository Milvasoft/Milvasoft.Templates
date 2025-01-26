using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;
using Milvonion.Application.Features.ContentManagement.Namespaces.CreateNamespace;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.CreateResourceGroup;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class CreateResourceGroupCommandValidator : AbstractValidator<CreateNamespaceCommand>
{
    ///<inheritdoc cref="CreateResourceGroupCommandValidator"/>
    public CreateResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName);
    }
}