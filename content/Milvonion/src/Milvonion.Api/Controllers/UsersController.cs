using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.UserDtos;
using Milvonion.Application.Features.Users.CreateUser;
using Milvonion.Application.Features.Users.DeleteUser;
using Milvonion.Application.Features.Users.GetUserDetail;
using Milvonion.Application.Features.Users.GetUserList;
using Milvonion.Application.Features.Users.UpdateUser;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// User endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion(GlobalConstant.CurrentApiVersion)]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class UsersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets users.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.List)]
    [HttpPatch]
    public Task<ListResponse<UserListDto>> GetUsersAsync(GetUserListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Get user according to user id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Detail)]
    [HttpGet("user")]
    public Task<Response<UserDetailDto>> GetUserAsync([FromQuery] GetUserDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Create)]
    [HttpPost("user")]
    public Task<Response<int>> AddUserAsync(CreateUserCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates user. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Update)]
    [HttpPut("user")]
    public Task<Response<int>> UpdateUserAsync(UpdateUserCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Removes user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Delete)]
    [HttpDelete("user")]
    public Task<Response<int>> RemoveUserAsync([FromQuery] DeleteUserCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);
}