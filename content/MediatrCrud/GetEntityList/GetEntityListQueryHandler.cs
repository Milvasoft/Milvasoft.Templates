using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using projectName.Application.Dtos.EntityDtos;

namespace projectName.Application.Features.pluralName.GetEntityList;

/// <summary>
/// Handles the entity list operation.
/// </summary>
/// <param name="entityRepository"></param>
public class GetEntityListQueryHandler(IprojectNameRepositoryBase<Entity> entityRepository) : IInterceptable, IListQueryHandler<GetEntityListQuery, EntityListDto>
{
    private readonly IprojectNameRepositoryBase<Entity> _entityRepository = entityRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<EntityListDto>> Handle(GetEntityListQuery request, CancellationToken cancellationToken)
    {
        var response = await _entityRepository.GetAllAsync(request, projection: EntityListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
