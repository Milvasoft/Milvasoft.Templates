using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using projectName.Application.Dtos.EntityDtos;

namespace projectName.Application.Features.pluralName.GetEntityDetail;

/// <summary>
/// Handles the entity detail operation.
/// </summary>
/// <param name="entityRepository"></param>
public class GetEntityDetailQueryHandler(IprojectNameRepositoryBase<Entity> entityRepository) : IInterceptable, IQueryHandler<GetEntityDetailQuery, EntityDetailDto>
{
    private readonly IprojectNameRepositoryBase<Entity> _entityRepository = entityRepository;

    /// <inheritdoc/>
    public async Task<Response<EntityDetailDto>> Handle(GetEntityDetailQuery request, CancellationToken cancellationToken)
    {
        var entity = await _entityRepository.GetByIdAsync(request.EntityId, null, EntityDetailDto.Projection, cancellationToken: cancellationToken);

        if (entity == null)
            return Response<EntityDetailDto>.Success(entity, MessageKey.EntityNotFound, MessageType.Warning);

        return Response<EntityDetailDto>.Success(entity);
    }
}
