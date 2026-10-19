using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;

namespace projectName.Application.Features.pluralName.DeleteEntity;

/// <summary>
/// Validations of <see cref="DeleteEntityCommand"/>.
/// </summary>
public sealed class DeleteEntityCommandValidator : AbstractValidator<DeleteEntityCommand>
{
    ///<inheritdoc cref="DeleteEntityCommandValidator"/>
    public DeleteEntityCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.EntityId)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Entity]]);
    }
}