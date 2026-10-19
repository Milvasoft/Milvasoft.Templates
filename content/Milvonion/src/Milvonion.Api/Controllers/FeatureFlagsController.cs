using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.FeatureFlagDtos;
using Milvonion.Application.Features.FeatureFlags.GetFeatureFlagList;
using Milvonion.Application.Features.FeatureFlags.UpdateFeatureFlag;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Admin control of the runtime kill switches (see FeatureFlagAttribute). A switched off feature answers HTTP 503 on
/// its endpoints from the next request on.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion(GlobalConstant.CurrentApiVersion)]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class FeatureFlagsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets feature flags.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.FeatureFlagManagement.List)]
    [HttpPatch]
    public Task<ListResponse<FeatureFlagListDto>> GetFeatureFlagsAsync(GetFeatureFlagListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates feature flag. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.FeatureFlagManagement.Update)]
    [HttpPut("featureFlag")]
    public Task<Response<int>> UpdateFeatureFlagAsync(UpdateFeatureFlagCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);
}
