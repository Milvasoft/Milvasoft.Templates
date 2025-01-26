using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class UpdateNamespaceCommandValidator : AbstractValidator<UpdateNamespaceCommand>
{
    ///<inheritdoc cref="UpdateNamespaceCommandValidator"/>
    public UpdateNamespaceCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Namespace]]);

        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(query => query.Name.IsUpdated);
    }
}