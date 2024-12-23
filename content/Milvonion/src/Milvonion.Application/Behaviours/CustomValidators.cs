using FluentValidation;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Custom validators.
/// </summary>
public static class CustomValidators
{
    /// <summary>
    /// Checks whether the value is greater than or equal to 21.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleBuilder"></param>
    /// <returns></returns>
    public static IRuleBuilderOptions<T, int> NotBeDefaultData<T>(this IRuleBuilder<T, int> ruleBuilder) => ruleBuilder.SetValidator(new DefaultDataValidator<T>());
}
