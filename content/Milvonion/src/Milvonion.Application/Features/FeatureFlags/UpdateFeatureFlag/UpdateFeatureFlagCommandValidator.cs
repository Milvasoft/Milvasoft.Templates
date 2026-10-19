using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Features.FeatureFlags.UpdateFeatureFlag;

/// <summary>
/// Validations of <see cref="UpdateFeatureFlagCommand"/>.
/// </summary>
public sealed class UpdateFeatureFlagCommandValidator : AbstractValidator<UpdateFeatureFlagCommand>
{
    private const int _reasonMaxLength = 200;

    ///<inheritdoc cref="UpdateFeatureFlagCommandValidator"/>
    public UpdateFeatureFlagCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.FeatureFlag]]);

        RuleFor(query => query.Reason.Value)
            .MaximumLength(_reasonMaxLength)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalReason], _reasonMaxLength])
            .When(query => query.Reason.IsUpdated);
    }
}
