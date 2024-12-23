using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

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
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Content]]);

        RuleFor(query => query.Value)
            .Must(name => name == null || (name.IsUpdated && !string.IsNullOrWhiteSpace(name.Value)))
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[nameof(UpdateContentCommand.Value)]]);

        RuleForEach(query => query.Medias.Value)
            .NotEmpty()
            .NotNull()
            .When(query => query.Value != null && query.Value.IsUpdated)
            .SetValidator(new UpsertMediaValidator(localizer));
    }
}