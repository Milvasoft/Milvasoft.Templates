using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Namespaces.CreateNamespace;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class CreateContentCommandValidator : AbstractValidator<CreateNamespaceCommand>
{
    ///<inheritdoc cref="CreateContentCommandValidator"/>
    public CreateContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(CreateNamespaceCommand.Name)]]);
    }
}