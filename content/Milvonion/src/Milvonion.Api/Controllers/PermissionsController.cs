using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.PermissionDtos;
using Milvonion.Application.Features.Permissions.GetPermissionList;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Permission endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
public class PermissionsController(IMediator mediator, IPermissionManager permissionManager) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IPermissionManager _permissionManager = permissionManager;

    /// <summary>
    /// Get permissions in the system.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.PermissionManagement.List)]
    [HttpPatch]
    public async Task<ListResponse<PermissionListDto>> GetPermissionsAsync(GetPermissionListQuery request, CancellationToken cancellation) => await _mediator.Send(request, cancellation);

    /// <summary>
    /// Migrates permissions to database.
    /// </summary>
    /// <returns></returns>
    [Auth(PermissionCatalog.App.SuperAdmin)]
    [HttpPut("migrate")]
    public async Task<Response<string>> MigratePermissionsAsync() => await _permissionManager.MigratePermissionsAsync(default);
}