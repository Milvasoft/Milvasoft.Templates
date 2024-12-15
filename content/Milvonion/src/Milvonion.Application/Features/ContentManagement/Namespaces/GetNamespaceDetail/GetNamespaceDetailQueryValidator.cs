using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceDetail;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetNamespaceDetailQueryValidator : AbstractValidator<GetNamespaceDetailQuery>
{
    ///<inheritdoc cref="GetNamespaceDetailQueryValidator"/>
    public GetNamespaceDetailQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.NamespaceId)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);
    }
}