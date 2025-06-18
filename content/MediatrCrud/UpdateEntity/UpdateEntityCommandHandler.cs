using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Ef.Transaction;
using Milvasoft.Interception.Interceptors.Logging;

namespace projectName.Application.Features.pluralName.UpdateEntity;

/// <summary>
/// Handles the update of the entity.
/// </summary>
/// <param name="EntityRepository"></param>
[Log]
[Transaction]
[UserActivityTrack(UserActivity.UpdateEntity)]
public record UpdateEntityCommandHandler(IprojectNameRepositoryBase<Entity> EntityRepository) : IInterceptable, ICommandHandler<UpdateEntityCommand, datatypefe>
{
    private readonly IprojectNameRepositoryBase<Entity> _entityRepository = EntityRepository;

    /// <inheritdoc/>
    public async Task<Response<datatypefe>> Handle(UpdateEntityCommand request, CancellationToken cancellationToken)
    {
        var setPropertyBuilder = _entityRepository.GetUpdatablePropertiesBuilder(request);

        await _entityRepository.ExecuteUpdateAsync(request.Id, setPropertyBuilder, cancellationToken: cancellationToken);

        return Response<datatypefe>.Success(request.Id);
    }
}
