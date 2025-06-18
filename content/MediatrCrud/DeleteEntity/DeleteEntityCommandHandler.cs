using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;

namespace projectName.Application.Features.pluralName.DeleteEntity;

/// <summary>
/// Handles the deletion of the entity.
/// </summary>
/// <param name="EntityRepository"></param>
[Log]
[UserActivityTrack(UserActivity.DeleteEntity)]
public record DeleteEntityCommandHandler(IprojectNameRepositoryBase<Entity> EntityRepository) : IInterceptable, ICommandHandler<DeleteEntityCommand, datatypefe>
{
    private readonly IprojectNameRepositoryBase<Entity> _entityRepository = EntityRepository;

    /// <inheritdoc/>
    public async Task<Response<datatypefe>> Handle(DeleteEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _entityRepository.GetForDeleteAsync(request.EntityId, cancellationToken: cancellationToken);

        if (entity == null)
            return Response<datatypefe>.Error(default, MessageKey.EntityNotFound);

        await _entityRepository.DeleteAsync(entity, cancellationToken: cancellationToken);

        return Response<datatypefe>.Success(request.EntityId);
    }
}
