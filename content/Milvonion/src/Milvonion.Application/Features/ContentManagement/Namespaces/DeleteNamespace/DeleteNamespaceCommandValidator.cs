using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

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
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.NamespaceId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}