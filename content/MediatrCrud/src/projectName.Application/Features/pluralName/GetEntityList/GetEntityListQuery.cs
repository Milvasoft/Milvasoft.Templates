using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using projectName.Application.Dtos.EntityDtos;

namespace projectName.Application.Features.pluralName.GetEntityList;

/// <summary>
/// Data transfer object for entity list.
/// </summary>
public record GetEntityListQuery : ListRequest, IListRequestQuery<EntityListDto>
{
}