using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using projectName.Application.Behaviours;

namespace projectName.Application.Features.pluralName.CreateEntity;

/// <summary>
/// Validations of <see cref="CreateEntityCommand"/>.
/// </summary>
public sealed class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    ///<inheritdoc cref="CreateEntityCommandValidator"/>
    public CreateEntityCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName);
    }
}