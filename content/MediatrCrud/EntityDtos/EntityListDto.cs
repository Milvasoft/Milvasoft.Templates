using Milvasoft.Attributes.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace projectName.Application.Dtos.EntityDtos;

/// <summary>
/// Data transfer object for entity list.
/// </summary>
[Translate]
public class EntityListDto : projectNameBaseDto<datatypefe>
{
    /// <summary>
    /// Name of the entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Projection expression for mapping Entity entity to EntityListDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<Entity, EntityListDto>> Projection { get; } = r => new EntityListDto
    {
        Id = r.Id,
    };
}
