using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.ActivityLogDtos;
using Milvonion.Application.Features.ActivityLogs.GetActivityLogList;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Activity Log endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class ActivityLogsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets user activities.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ActivityLogManagement.List)]
    [HttpPatch]
    public Task<ListResponse<ActivityLogListDto>> GetRolesAsync(GetActivityLogListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);
}