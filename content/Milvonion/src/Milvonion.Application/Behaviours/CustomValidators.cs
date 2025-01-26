using FluentValidation;
using FluentValidation.Validators;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Types.Structs;
using Milvonion.Application.Dtos.TranslationDtos;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Default data validator.
/// </summary>
public class DefaultDataValidator<T> : PropertyValidator<T, int>
{
    /// <inheritdoc/>
    public override string Name => "DefaultDataValidator";

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} should not less than 21.";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, int value) => value <= 0 || value >= 21;
}

/// <summary>
/// Feature query validations. 
/// </summary>
public sealed class NameDescriptionTranslationDtoValidator : AbstractValidator<NameDescriptionTranslationDto>
{
    ///<inheritdoc cref="NameDescriptionTranslationDtoValidator"/>
    public NameDescriptionTranslationDtoValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .MaximumLength(100)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalName], 100]);

        RuleFor(query => query.Description)
            .MaximumLength(5000)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalDescription], 5000]);

        RuleFor(query => query.LanguageId)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify])
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Language]]);
    }
}

/// <summary>
/// Feature query validations. 
/// </summary>
public sealed class UpdateNameDescriptionTranslationDtoValidator : AbstractValidator<UpdateProperty<List<NameDescriptionTranslationDto>>>
{
    ///<inheritdoc cref="UpdateNameDescriptionTranslationDtoValidator"/>
    public UpdateNameDescriptionTranslationDtoValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(q => q.IsUpdated);

        RuleForEach(query => query.Value)
            .SetValidator(new NameDescriptionTranslationDtoValidator(localizer))
            .When(query => query.IsUpdated);
    }
}