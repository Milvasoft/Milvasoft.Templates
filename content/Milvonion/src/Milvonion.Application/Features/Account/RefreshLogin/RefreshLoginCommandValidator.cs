﻿using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Features.Account.Logout;

namespace Milvonion.Application.Features.Account.RefreshLogin;

/// <summary>
/// Account detail query validations. 
/// </summary>
public sealed class RefreshLoginCommandValidator : AbstractValidator<RefreshLoginCommand>
{
    ///<inheritdoc cref="LogoutCommandValidator"/>
    public RefreshLoginCommandValidator(IMilvaLocalizer localizer)
    {
        RuleFor(query => query.UserName)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.IdentityInvalidUsername]);

        RuleFor(query => query.DeviceId)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.PossibleUIError]);

        RuleFor(query => query.RefreshToken)
            .NotEmpty()
            .NotNull()
            .WithMessage(localizer[MessageKey.PossibleUIError]);
    }
}