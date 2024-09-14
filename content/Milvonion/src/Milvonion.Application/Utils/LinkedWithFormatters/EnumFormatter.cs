using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Utils.LinkedWithFormatters;

/// <summary>
/// Converts enum value to its localized display name.
/// </summary>
/// <typeparam name="TEnum"></typeparam>
/// <param name="milvaLocalizer"></param>
public class EnumFormatter<TEnum>(IMilvaLocalizer milvaLocalizer) : ILinkedWithFormatter where TEnum : Enum
{
    private readonly IMilvaLocalizer _milvaLocalizer = milvaLocalizer;

    /// <inheritdoc/>
    public const string FormatterName = "Enum";

    /// <inheritdoc/>
    public object Format(object value)
    {
        if (value is TEnum enumValue)
        {
            var resourceKey = $"{typeof(TEnum).Name}.{enumValue}";

            return _milvaLocalizer[resourceKey].ToString();
        }

        return value?.ToString();
    }
}
