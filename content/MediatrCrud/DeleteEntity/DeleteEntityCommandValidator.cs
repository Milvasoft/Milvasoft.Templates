using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using projectName.Application.Behaviours;

namespace projectName.Application.Features.pluralName.DeleteEntity;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class DeleteEntityCommandValidator : AbstractValidator<DeleteEntityCommand>
{
    ///<inheritdoc cref="DeleteEntityCommandValidator"/>
    public DeleteEntityCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.EntityId)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.EntityId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Entity]]);
    }
}