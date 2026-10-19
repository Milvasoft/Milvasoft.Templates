using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;

/// <summary>
/// Validations of <see cref="UpdateNamespaceCommand"/>.
/// </summary>
public sealed class UpdateNamespaceCommandValidator : AbstractValidator<UpdateNamespaceCommand>
{
    ///<inheritdoc cref="UpdateNamespaceCommandValidator"/>
    public UpdateNamespaceCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);

        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(query => query.Name.IsUpdated);
    }
}