using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupDetail;

/// <summary>
/// Validations of <see cref="GetResourceGroupDetailQuery"/>.
/// </summary>
public sealed class GetResourceGroupDetailQueryValidator : AbstractValidator<GetResourceGroupDetailQuery>
{
    ///<inheritdoc cref="GetResourceGroupDetailQueryValidator"/>
    public GetResourceGroupDetailQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.ResourceGroupId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}