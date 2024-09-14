using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.RoleDtos;

namespace Milvonion.Application.Features.Roles.GetRoleList;

/// <summary>
/// Data transfer object for role list.
/// </summary>
public record GetRoleListQuery : ListRequest, IListRequestQuery<RoleListDto>
{
}