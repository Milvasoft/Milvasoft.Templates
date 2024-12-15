using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.Roles.UpdateRole;

namespace Milvonion.Application.Features.Languages.UpdateLanguage;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    ///<inheritdoc cref="UpdateRoleCommandValidator"/>
    public UpdateLanguageCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotEqual(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Language]]);
    }
}