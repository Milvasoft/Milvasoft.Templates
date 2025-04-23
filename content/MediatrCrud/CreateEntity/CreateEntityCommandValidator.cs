using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using projectName.Application.Behaviours;

namespace projectName.Application.Features.pluralName.CreateEntity;

/// <summary>
/// Account detail query validations. 
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