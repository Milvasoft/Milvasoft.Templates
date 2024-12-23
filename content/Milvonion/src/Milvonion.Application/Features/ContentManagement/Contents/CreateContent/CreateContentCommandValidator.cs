using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class CreateContentCommandValidator : AbstractValidator<CreateContentCommand>
{
    ///<inheritdoc cref="CreateContentCommandValidator"/>
    public CreateContentCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(dto => dto).SetValidator(new CreateContentDtoValidator(localizer));
    }
}