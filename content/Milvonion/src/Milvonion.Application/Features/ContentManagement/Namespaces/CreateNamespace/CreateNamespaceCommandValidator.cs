using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.CreateNamespace;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class CreateNamespaceCommandValidator : AbstractValidator<CreateNamespaceCommand>
{
    ///<inheritdoc cref="CreateNamespaceCommandValidator"/>
    public CreateNamespaceCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(CreateNamespaceCommand.Name)]]);
    }
}