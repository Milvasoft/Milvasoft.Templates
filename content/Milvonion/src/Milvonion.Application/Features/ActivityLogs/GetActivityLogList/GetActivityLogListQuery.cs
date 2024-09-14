using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ActivityLogDtos;

namespace Milvonion.Application.Features.ActivityLogs.GetActivityLogList;

/// <summary>
/// Data transfer object for user activity log list.
/// </summary>
public record GetActivityLogListQuery : ListRequest, IListRequestQuery<ActivityLogListDto>
{
}
