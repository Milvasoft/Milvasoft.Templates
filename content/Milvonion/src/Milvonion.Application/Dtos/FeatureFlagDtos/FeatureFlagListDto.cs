using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Milvonion.Application.Dtos.FeatureFlagDtos;

/// <summary>
/// A kill switch shown on the feature flag management screen.
/// </summary>
public class FeatureFlagListDto : MilvonionBaseDto<int>
{
    /// <summary>
    /// Flag key, one of <see cref="FeatureFlagKeys"/>.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Whether the feature is currently on.
    /// </summary>
    [DisplayFormat("{isEnabledDescription}")]
    [Filterable(FilterComponentType = UiInputConstant.SwitchInput)]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Localized yes/no text of <see cref="IsEnabled"/>.
    /// </summary>
    [Filterable(false)]
    [Browsable(false)]
    [LinkedWith<YesNoFormatter>(nameof(IsEnabled), YesNoFormatter.FormatterName)]
    public string IsEnabledDescription { get; set; }

    /// <summary>
    /// Optional short reason for the switch; null when enabled.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Projection expression for mapping FeatureFlag entity to FeatureFlagListDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<FeatureFlag, FeatureFlagListDto>> Projection { get; } = f => new FeatureFlagListDto
    {
        Id = f.Id,
        Key = f.Key,
        IsEnabled = f.IsEnabled,
        Reason = f.Reason,
    };
}
