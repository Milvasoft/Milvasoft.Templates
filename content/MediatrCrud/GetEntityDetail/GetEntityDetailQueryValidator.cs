using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace projectName.Application.Features.pluralName.GetEntityDetail;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class GetEntityDetailQueryValidator : AbstractValidator<GetEntityDetailQuery>
{
    ///<inheritdoc cref="GetEntityDetailQueryValidator"/>
    public GetEntityDetailQueryValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.EntityId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Entity]]);
    }
}