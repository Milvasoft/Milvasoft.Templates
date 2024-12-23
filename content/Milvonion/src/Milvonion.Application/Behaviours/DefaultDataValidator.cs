using FluentValidation;
using FluentValidation.Validators;

namespace Milvonion.Application.Behaviours;

/// <summary>
/// Default data validator.
/// </summary>
public class DefaultDataValidator<T> : PropertyValidator<T, int>
{
    /// <inheritdoc/>
    public override string Name => "DefaultDataValidator";

    /// <inheritdoc/>
    protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} should not less than 21.";

    /// <inheritdoc/>
    public override bool IsValid(ValidationContext<T> context, int value) => value <= 0 || value >= 21;
}