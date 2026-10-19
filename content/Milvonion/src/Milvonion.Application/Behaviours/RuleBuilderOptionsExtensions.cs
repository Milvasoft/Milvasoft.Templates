using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Identity.Abstract;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Custom validators.
/// </summary>
public static class RuleBuilderOptionsExtensions
{
    /// <summary>
    /// E.164 form every phone number sent to the API must match: a "+", the country code and the subscriber number,
    /// digits only (at most 15).
    /// </summary>
    public const string E164PhoneNumberPattern = @"^\+[1-9]\d{6,14}$";

    /// <summary>
    /// Checks whether the value is greater than or equal to 21.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="rangeMin"></param>
    /// <param name="rangeMax"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, int> NotBeDefaultData<T>(this IRuleBuilder<T, int> ruleBuilder, int rangeMin = 0, int rangeMax = 21) => ruleBuilder.SetValidator(new DefaultDataValidator<T>(rangeMin, rangeMax));

    /// <summary>
    /// Checks whether the value is greater than or equal to 21.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="rangeMin"></param>
    /// <param name="rangeMax"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, int> NotBeDefaultData<T>(this IRuleBuilderOptions<T, int> ruleBuilder, int rangeMin = 0, int rangeMax = 21) => ruleBuilder.SetValidator(new DefaultDataValidator<T>(rangeMin, rangeMax));

    /// <summary>
    /// Checks whether the value is greater than or <see cref="GlobalConstant.AutoIncrementStart"/>..
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="rangeMin"></param>
    /// <param name="rangeMax"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, long> NotBeDefaultData<T>(this IRuleBuilder<T, long> ruleBuilder, long rangeMin = 0, long rangeMax = GlobalConstant.AutoIncrementStart) => ruleBuilder.SetValidator(new DefaultDataLongValidator<T>(rangeMin, rangeMax));

    /// <summary>
    /// Checks whether the value is greater than or <see cref="GlobalConstant.AutoIncrementStart"/>..
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="rangeMin"></param>
    /// <param name="rangeMax"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, long> NotBeDefaultData<T>(this IRuleBuilderOptions<T, long> ruleBuilder, long rangeMin = 0, long rangeMax = GlobalConstant.AutoIncrementStart) => ruleBuilder.SetValidator(new DefaultDataLongValidator<T>(rangeMin, rangeMax));

    /// <summary>
    /// Checks whether the value is valid email address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.EmailAddress().WithMessage(localizer[LocalizerKeys.IdentityInvalidEmail]);

    /// <summary>
    /// Checks whether the value is valid email address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.EmailAddress().WithMessage(localizer[LocalizerKeys.IdentityInvalidEmail]);

    /// <summary>
    /// Checks whether the value is a valid phone number in the E.164 form (<see cref="E164PhoneNumberPattern"/>).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(E164PhoneNumberPattern).WithMessage(localizer[MessageKey.InvalidPhoneNumber]);

    /// <summary>
    /// Checks whether the value is a valid phone number in the E.164 form (<see cref="E164PhoneNumberPattern"/>).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(E164PhoneNumberPattern).WithMessage(localizer[MessageKey.InvalidPhoneNumber]);

    /// <summary>
    /// Checks the value against the configured Identity password policy (length, digit, casing, symbol), so the
    /// request is rejected with the localized policy message before any token or database work.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="userManager"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> IdentityPassword<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaUserManager<User, int> userManager, IMilvaLocalizer localizer)
        => ruleBuilder.Must(password => userManager.ValidatePassword(password) is null)
                      .WithMessage((_, password) => localizer[userManager.ValidatePassword(password)]);

    /// <summary>
    /// Checks the value against the configured Identity password policy (length, digit, casing, symbol), so the
    /// request is rejected with the localized policy message before any token or database work.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="userManager"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> IdentityPassword<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaUserManager<User, int> userManager, IMilvaLocalizer localizer)
        => ruleBuilder.Must(password => userManager.ValidatePassword(password) is null)
                      .WithMessage((_, password) => localizer[userManager.ValidatePassword(password)]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> UrlAddress<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(@"^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$").WithMessage(localizer[MessageKey.InvalidUrlAddress]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> UrlAddress<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(@"^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$").WithMessage(localizer[MessageKey.InvalidUrlAddress]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> NotNullOrEmpty<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaLocalizer localizer, string key)
        => ruleBuilder.NotEmpty()
                      .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[key]])
                      .NotNull()
                      .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[key]]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> NotNullOrEmpty<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaLocalizer localizer, string key)
        => ruleBuilder.NotEmpty()
                      .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[key]])
                      .NotNull()
                      .WithMessage(localizer[MessageKey.CannotBeEmpty, localizer[key]]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> NotNullOrEmpty<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, IMilvaLocalizer localizer, string key)
        => ruleBuilder.NotEmpty()
                      .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[key]])
                      .NotNull()
                      .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[key]]);

    /// <summary>
    /// Checks whether the value is valid website address.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, TProperty> NotNullOrEmpty<T, TProperty>(this IRuleBuilderOptions<T, TProperty> ruleBuilder, IMilvaLocalizer localizer, string key)
        => ruleBuilder.NotEmpty()
                      .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[key]])
                      .NotNull()
                      .WithMessage(localizer[MessageKey.PleaseSendCorrect, localizer[key]]);
}
