using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

/// <summary>
/// Validations of <see cref="CreateContentCommand"/>.
/// </summary>
public sealed class CreateContentCommandValidator : AbstractValidator<CreateContentCommand>
{
    ///<inheritdoc cref="CreateContentCommandValidator"/>
    public CreateContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(dto => dto).SetValidator(new CreateContentDtoValidator(localizer));
    }
}