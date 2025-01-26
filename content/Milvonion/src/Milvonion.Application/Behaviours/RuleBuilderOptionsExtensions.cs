using FluentValidation;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Custom validators.
/// </summary>
public static class RuleBuilderOptionsExtensions
{
    /// <summary>
    /// Checks whether the value is greater than or equal to 21.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, int> NotBeDefaultData<T>(this IRuleBuilder<T, int> ruleBuilder) => ruleBuilder.SetValidator(new DefaultDataValidator<T>());

    /// <summary>
    /// Checks whether the value is greater than or equal to 21.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, int> NotBeDefaultData<T>(this IRuleBuilderOptions<T, int> ruleBuilder) => ruleBuilder.SetValidator(new DefaultDataValidator<T>());

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
    /// Checks whether the value is valid phone number.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(@"^\+90\d{10}$").WithMessage(localizer[MessageKey.InvalidPhoneNumber]);

    /// <summary>
    /// Checks whether the value is valid phone number.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <param name="localizer"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, string> PhoneNumber<T>(this IRuleBuilderOptions<T, string> ruleBuilder, IMilvaLocalizer localizer)
        => ruleBuilder.Matches(@"^\+90\d{10}$").WithMessage(localizer[MessageKey.InvalidPhoneNumber]);

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
