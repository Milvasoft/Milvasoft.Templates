using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Identity.Abstract;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.Users.UpdateUser;

/// <summary>
/// Validations of <see cref="UpdateUserCommand"/>.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    ///<inheritdoc cref="UpdateUserCommandValidator"/>
    public UpdateUserCommandValidator(IMilvaLocalizer localizer, IMilvaUserManager<User, int> userManager)
    {
        RuleFor(query => query.Id)
            .NotBeDefaultData()
            .WithMessage(localizer[MessageKey.DefaultValueCannotModify]);

        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.User]]);

        RuleFor(query => query.Name.Value)
            .MaximumLength(User.NameMaxLength)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalName], User.NameMaxLength])
            .When(q => q.Name.IsUpdated);

        RuleFor(query => query.Surname.Value)
            .MaximumLength(User.NameMaxLength)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalSurname], User.NameMaxLength])
            .When(q => q.Surname.IsUpdated);

        RuleFor(query => query.NewPassword.Value)
            .IdentityPassword(userManager, localizer)
            .When(q => q.NewPassword.IsUpdated);
    }
}