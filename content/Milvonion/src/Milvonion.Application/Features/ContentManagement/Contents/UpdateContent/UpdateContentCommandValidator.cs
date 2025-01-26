using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;
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
            .NotNullOrEmpty(localizer, MessageKey.GlobalValue)
            .When(q => q.Value.IsUpdated);

        RuleForEach(query => query.Medias.Value)
            .NotNullOrEmpty(localizer, MessageKey.Media)
            .When(query => query.Value != null && query.Value.IsUpdated)
            .SetValidator(new UpsertMediaValidator(localizer));
    }
}