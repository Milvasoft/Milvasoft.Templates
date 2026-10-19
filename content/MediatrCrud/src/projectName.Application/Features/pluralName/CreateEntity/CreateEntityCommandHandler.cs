using Mapster;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Interceptors.Logging;

namespace projectName.Application.Features.pluralName.CreateEntity;

/// <summary>
/// Handles the creation of the entity.
/// </summary>
/// <param name="EntityRepository"></param>
[Log]
[UserActivityTrack(UserActivity.CreateEntity)]
public record CreateEntityCommandHandler(IprojectNameRepositoryBase<Entity> EntityRepository) : IInterceptable, ICommandHandler<CreateEntityCommand, datatypefe>
{
    private readonly IprojectNameRepositoryBase<Entity> _entityRepository = EntityRepository;

    /// <inheritdoc/>
    public async Task<Response<datatypefe>> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<Entity>();

        await _entityRepository.AddAsync(entity, cancellationToken);

        return Response<datatypefe>.Success(entity.Id);
    }
}
