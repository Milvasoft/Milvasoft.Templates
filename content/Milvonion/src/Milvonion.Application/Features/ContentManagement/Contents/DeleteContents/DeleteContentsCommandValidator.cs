using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.Contents.DeleteContents;

/// <summary>
/// Validations of <see cref="DeleteContentsCommand"/>.
/// </summary>
public sealed class DeleteContentsCommandValidator : AbstractValidator<DeleteContentsCommand>
{
    ///<inheritdoc cref="DeleteContentsCommandValidator"/>
    public DeleteContentsCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ContentIdList)
            .NotNullOrEmpty(localizer, MessageKey.Content);

        RuleForEach(query => query.ContentIdList)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Content]]);
    }
}