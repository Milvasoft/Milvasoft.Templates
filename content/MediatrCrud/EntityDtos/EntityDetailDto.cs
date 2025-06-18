using Milvasoft.Attributes.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace projectName.Application.Dtos.EntityDtos;

/// <summary>
/// Data transfer object for entity details.
/// </summary>
[Translate]
[ExcludeFromMetadata]
public class EntityDetailDto : projectNameBaseDto<datatypefe>
{
    /// <summary>
    /// Name of entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Information about record audit.
    /// </summary>
    public AuditDto<datatypefe> AuditInfo { get; set; }

    /// <summary>
    /// Projection expression for mapping Entity entity to EntityDetailDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<Entity, EntityDetailDto>> Projection { get; } = r => new EntityDetailDto
    {
        Id = r.Id,
        AuditInfo = new AuditDto<datatypefe>(r)
    };
}
