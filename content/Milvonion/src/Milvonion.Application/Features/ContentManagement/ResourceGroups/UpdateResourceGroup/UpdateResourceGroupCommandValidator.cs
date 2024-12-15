using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;

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
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.ResourceGroup]]);

        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .When(query => query.Name != null && query.Name.IsUpdated)
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateNamespaceCommand.Name)]]);
    }
}