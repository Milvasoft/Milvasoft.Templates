using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvonion.Application.Dtos.ActivityLogDtos;

namespace Milvonion.Application.Features.ActivityLogs.GetActivityLogList;

/// <summary>
/// Get all activity logs.
/// </summary>
/// <param name="activityLogRepository"></param>
public class GetActivityLogListQueryHandler(IMilvonionRepositoryBase<ActivityLog> activityLogRepository) : IInterceptable, IListQueryHandler<GetActivityLogListQuery, ActivityLogListDto>
{
    private readonly IMilvonionRepositoryBase<ActivityLog> _activityLogRepository = activityLogRepository;

    /// <inheritdoc/>
    public async Task<ListResponse<ActivityLogListDto>> Handle(GetActivityLogListQuery request, CancellationToken cancellationToken)
    {
        var sortRequest = request?.Sorting;

        if (sortRequest == null && request != null)
            request.Sorting = new Milvasoft.Components.Rest.Request.SortRequest()
            {
                SortBy = nameof(ActivityLogListDto.ActivityDate),
                Type = SortType.Desc
            };

        var response = await _activityLogRepository.GetAllAsync(request, projection: ActivityLogListDto.Projection, cancellationToken: cancellationToken);

        return response;
    }
}
