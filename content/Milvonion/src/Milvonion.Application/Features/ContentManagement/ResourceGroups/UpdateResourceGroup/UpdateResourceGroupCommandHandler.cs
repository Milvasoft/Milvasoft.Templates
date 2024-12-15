using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Ef.Transaction;
using Milvasoft.Interception.Interceptors.Logging;
using Milvonion.Domain.ContentManagement;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.UpdateResourceGroup;

/// <summary>
/// Handles the update of the resource group.
/// </summary>
/// <param name="ResourceGroupRepository"></param>
[Log]
[Transaction]
[UserActivityTrack(UserActivity.UpdateResourceGroup)]
public record UpdateResourceGroupCommandHandler(IMilvonionRepositoryBase<ResourceGroup> ResourceGroupRepository) : IInterceptable, ICommandHandler<UpdateResourceGroupCommand, int>
{
    private readonly IMilvonionRepositoryBase<ResourceGroup> _resourceGroupRepository = ResourceGroupRepository;

    /// <inheritdoc/>
    public async Task<Response<int>> Handle(UpdateResourceGroupCommand request, CancellationToken cancellationToken)
    {
        var setPropertyBuilder = _resourceGroupRepository.GetUpdatablePropertiesBuilder(request);

        await _resourceGroupRepository.ExecuteUpdateAsync(request.Id, setPropertyBuilder, cancellationToken: cancellationToken);

        return Response<int>.Success(request.Id);
    }
}
