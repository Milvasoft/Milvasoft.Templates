using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.FeatureFlagDtos;

namespace Milvonion.Application.Features.FeatureFlags.GetFeatureFlagList;

/// <summary>
/// Handles the feature flag list operation. Every key of <see cref="FeatureFlagKeys.All"/> has a row (seeded on startup).
/// </summary>
/// <param name="featureFlagRepository"></param>
public class GetFeatureFlagListQueryHandler(IMilvonionRepositoryBase<FeatureFlag> featureFlagRepository) : IInterceptable, IListQueryHandler<GetFeatureFlagListQuery, FeatureFlagListDto>
{
    private readonly IMilvonionRepositoryBase<FeatureFlag> _featureFlagRepository = featureFlagRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<FeatureFlagListDto>> Handle(GetFeatureFlagListQuery request, CancellationToken cancellationToken)
    {
        request.PageNumber ??= 1;
        request.RowCount = request.RowCount.ClampRowCount(20);

        var response = await _featureFlagRepository.GetAllAsync(request, projection: FeatureFlagListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
