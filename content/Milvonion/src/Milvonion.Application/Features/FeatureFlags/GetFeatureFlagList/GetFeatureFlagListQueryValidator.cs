using FluentValidation;

namespace Milvonion.Application.Features.FeatureFlags.GetFeatureFlagList;

/// <summary>
/// Validations of <see cref="GetFeatureFlagListQuery"/>.
/// </summary>
public sealed class GetFeatureFlagListQueryValidator : AbstractValidator<GetFeatureFlagListQuery>
{
    ///<inheritdoc cref="GetFeatureFlagListQueryValidator"/>
    public GetFeatureFlagListQueryValidator()
    {
    }
}
