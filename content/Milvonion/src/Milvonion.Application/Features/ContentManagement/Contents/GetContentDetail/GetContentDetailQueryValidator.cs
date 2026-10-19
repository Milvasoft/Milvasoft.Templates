using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentDetail;

/// <summary>
/// Validations of <see cref="GetContentDetailQuery"/>.
/// </summary>
public sealed class GetContentDetailQueryValidator : AbstractValidator<GetContentDetailQuery>
{
    ///<inheritdoc cref="GetContentDetailQueryValidator"/>
    public GetContentDetailQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ContentId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Content]]);
    }
}