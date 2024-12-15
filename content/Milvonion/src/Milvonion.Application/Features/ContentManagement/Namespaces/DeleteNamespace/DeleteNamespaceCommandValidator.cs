using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.DeleteNamespace;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class DeleteNamespaceCommandValidator : AbstractValidator<DeleteNamespaceCommand>
{
    ///<inheritdoc cref="DeleteNamespaceCommandValidator"/>
    public DeleteNamespaceCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.NamespaceId)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}