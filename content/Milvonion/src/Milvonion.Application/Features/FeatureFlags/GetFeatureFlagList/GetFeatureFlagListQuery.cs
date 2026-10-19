using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.FeatureFlagDtos;

namespace Milvonion.Application.Features.FeatureFlags.GetFeatureFlagList;

/// <summary>
/// Data transfer object for feature flag list.
/// </summary>
public record GetFeatureFlagListQuery : ListRequest, IListRequestQuery<FeatureFlagListDto>
{
}
