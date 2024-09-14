using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.Pages.GetPageAccessibilityForCurrentUser;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetPageAccessibilityForCurrentUserQueryValidator : AbstractValidator<GetPageAccessibilityForCurrentUserQuery>
{
    ///<inheritdoc cref="GetPageAccessibilityForCurrentUserQueryValidator"/>
    public GetPageAccessibilityForCurrentUserQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.PageName)
            .Must(q => !string.IsNullOrEmpty(q))
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Page]]);
    }
}