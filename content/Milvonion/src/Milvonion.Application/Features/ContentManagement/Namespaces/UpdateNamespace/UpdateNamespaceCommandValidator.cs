using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateNamespaceCommandValidator : AbstractValidator<UpdateNamespaceCommand>
{
    ///<inheritdoc cref="UpdateNamespaceCommandValidator"/>
    public UpdateNamespaceCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);

        RuleFor(query => query.Name)
            .NotEmpty()
            .NotNull()
            .When(query => query.Name != null && query.Name.IsUpdated)
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateNamespaceCommand.Name)]]);
    }
}