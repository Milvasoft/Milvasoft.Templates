using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;

namespace Milvonion.Application.Features.ContentManagement.Contents.UpdateContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class UpdateContentCommandValidator : AbstractValidator<UpdateContentCommand>
{
    ///<inheritdoc cref="UpdateContentCommandValidator"/>
    public UpdateContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Content]]);

        RuleFor(query => query.Value)
            .NotEmpty()
            .NotNull()
            .When(query => query.Value != null && query.Value.IsUpdated)
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateNamespaceCommand.Name)]]);
    }
}