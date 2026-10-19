using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Identity.Abstract;
using Milvonion.Application.Behaviours;

namespace Milvonion.Application.Features.Users.CreateUser;

/// <summary>
/// Validations of <see cref="CreateUserCommand"/>.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    ///<inheritdoc cref="CreateUserCommandValidator"/>
    public CreateUserCommandValidator(IMilvaLocalizer localizer, IMilvaUserManager<User, int> userManager)
    {
        RuleFor(query => query.UserName)
            .NotNullOrEmpty(localizer, MessageKey.GlobalUsername);

        RuleFor(query => query.Password)
            .NotNullOrEmpty(localizer, MessageKey.GlobalPassword)
            .IdentityPassword(userManager, localizer);

        RuleFor(query => query.Name)
            .MaximumLength(User.NameMaxLength)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalName], User.NameMaxLength]);

        RuleFor(query => query.Surname)
            .MaximumLength(User.NameMaxLength)
            .WithMessage(localizer[MessageKey.MaxLengthReached, localizer[MessageKey.GlobalSurname], User.NameMaxLength]);

        RuleFor(query => query.UserType)
            .IsInEnum()
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.GlobalUserType]]);

        RuleFor(query => query.RoleIdList)
            .NotNullOrEmpty(localizer, MessageKey.Role);

        RuleForEach(query => query.RoleIdList)
            .GreaterThan(0)
            .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[MessageKey.Role]]);
    }
}