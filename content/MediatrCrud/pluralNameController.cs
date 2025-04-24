using projectName.Application.Dtos.EntityDtos;
using projectName.Application.Features.pluralName.CreateEntity;
using projectName.Application.Features.pluralName.DeleteEntity;
using projectName.Application.Features.pluralName.GetEntityDetail;
using projectName.Application.Features.pluralName.GetEntityList;
using projectName.Application.Features.pluralName.UpdateEntity;
using projectName.Application.Utils.Attributes;
using projectName.Application.Utils.Constants;
using projectName.Application.Utils.PermissionManager;
using projectName.Domain.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;

namespace projectName.Api.Controllers;

/// <summary>
/// Entity endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class pluralNameController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets pluralName.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.EntityManagement.List)]
    [HttpPatch]
    public Task<ListResponse<EntityListDto>> GetpluralNameAsync(GetEntityListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Get Entity according to Entity id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.EntityManagement.Detail)]
    [HttpGet("Entity")]
    public Task<Response<EntityDetailDto>> GetEntityAsync([FromQuery] GetEntityDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds Entity.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.EntityManagement.Create)]
    [HttpPost("Entity")]
    public Task<Response<int>> AddEntityAsync(CreateEntityCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates Entity. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.EntityManagement.Update)]
    [HttpPut("Entity")]
    public Task<Response<int>> UpdateEntityAsync(UpdateEntityCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Removes Entity.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.EntityManagement.Delete)]
    [HttpDelete("Entity")]
    public Task<Response<int>> RemoveEntityAsync([FromQuery] DeleteEntityCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);
}