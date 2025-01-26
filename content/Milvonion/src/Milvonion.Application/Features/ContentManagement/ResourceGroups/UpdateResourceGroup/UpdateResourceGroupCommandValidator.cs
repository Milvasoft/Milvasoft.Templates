﻿using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.UpdateResourceGroup;

/// <summary>
/// Query validations. 
/// </summary>
public sealed class UpdateResourceGroupCommandValidator : AbstractValidator<UpdateResourceGroupCommand>
{
    ///<inheritdoc cref="UpdateResourceGroupCommandValidator"/>
    public UpdateResourceGroupCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.Id)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.ResourceGroup]]);

        RuleFor(query => query.Name)
            .NotNullOrEmpty(localizer, MessageKey.GlobalName)
            .When(query => query.Name.IsUpdated);
    }
}