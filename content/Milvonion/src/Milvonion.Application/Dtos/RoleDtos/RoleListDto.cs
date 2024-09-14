using Mapster;
using Mapster.Models;
using Milvasoft.Attributes.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Milvonion.Application.Dtos.RoleDtos;

/// <summary>
/// Data transfer object for role list.
/// </summary>
[Translate]
public class RoleListDto : MilvonionBaseDto<int>
{
    /// <summary>
    /// Name of the role.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Projection expression for mapping Role entity to RoleListDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<Role, RoleListDto>> Projection { get; }
    = (Expression<Func<Role, RoleListDto>>)TypeAdapterConfig.GlobalSettings
                                                              .Default
                                                              .Config.CreateMapExpression(new TypeTuple(typeof(Role),
                                                                                                        typeof(RoleListDto)), MapType.Projection);
}
