using Milvasoft.Components.CQRS.Query;
using projectName.Application.Dtos.EntityDtos;

namespace projectName.Application.Features.pluralName.GetEntityDetail;

/// <summary>
/// Data transfer object for entity details.
/// </summary>
public record GetEntityDetailQuery : IQuery<EntityDetailDto>
{
    /// <summary>
    /// Entity id to access details.
    /// </summary>
    public datatypefe EntityId { get; set; }
}
