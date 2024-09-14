using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.PermissionDtos;

namespace Milvonion.Application.Features.Permissions.GetPermissionList;

/// <summary>
/// Data transfer object for permission details.
/// </summary>
public record GetPermissionListQuery : ListRequest, IListRequestQuery<PermissionListDto>
{
}
