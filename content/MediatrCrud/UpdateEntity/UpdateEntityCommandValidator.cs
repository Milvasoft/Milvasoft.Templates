using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using projectName.Application.Behaviours;

namespace projectName.Application.Features.pluralName.UpdateEntity;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateEntityCommandValidator : AbstractValidator<UpdateEntityCommand>
{
    ///<inheritdoc cref="UpdateEntityCommandValidator"/>
    public UpdateEntityCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Entity]]);

        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(q => q.Name.IsUpdated);
    }
}