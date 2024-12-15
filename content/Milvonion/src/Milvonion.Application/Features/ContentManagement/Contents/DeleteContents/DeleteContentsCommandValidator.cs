using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Contents.DeleteContents;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class DeleteContentsCommandValidator : AbstractValidator<DeleteContentsCommand>
{
    ///<inheritdoc cref="DeleteContentsCommandValidator"/>
    public DeleteContentsCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ContentIdList)
            .NotEmpty()
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Content]]);
    }
}