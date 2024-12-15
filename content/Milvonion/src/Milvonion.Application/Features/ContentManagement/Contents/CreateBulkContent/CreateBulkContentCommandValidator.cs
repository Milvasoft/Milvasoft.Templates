using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class CreateBulkContentCommandValidator : AbstractValidator<CreateBulkContentCommand>
{
    ///<inheritdoc cref="CreateContentCommandValidator"/>
    public CreateBulkContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Contents)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[MessageKey.Content]]);
    }
}