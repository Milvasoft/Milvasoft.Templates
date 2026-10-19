using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;

namespace Milvonion.Application.Features.FeatureFlags.UpdateFeatureFlag;

/// <summary>
/// Handles the update of the feature flag. The cached switch states are dropped after the update, so the guarded
/// endpoints apply the change on the next request.
/// </summary>
/// <param name="FeatureFlagRepository"></param>
/// <param name="FeatureFlagService"></param>
[Log]
[UserActivityTrack(UserActivity.UpdateFeatureFlag)]
public record UpdateFeatureFlagCommandHandler(IMilvonionRepositoryBase<FeatureFlag> FeatureFlagRepository, IFeatureFlagService FeatureFlagService) : IInterceptable, ICommandHandler<UpdateFeatureFlagCommand, int>
{
    private readonly IMilvonionRepositoryBase<FeatureFlag> _featureFlagRepository = FeatureFlagRepository;
    private readonly IFeatureFlagService _featureFlagService = FeatureFlagService;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(UpdateFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var setPropertyBuilder = _featureFlagRepository.GetUpdatablePropertiesBuilder(request);

        var updatedCount = await _featureFlagRepository.ExecuteUpdateAsync(request.Id, setPropertyBuilder, cancellationToken: cancellationToken);

        if (updatedCount == 0)
            return Response<int>.Error(0, MessageKey.FeatureFlagNotFound);

        await _featureFlagService.InvalidateAsync();

        return Response<int>.Success(request.Id);
    }
}
