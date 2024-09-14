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
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class UsersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets users.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.List)]
    [HttpPatch]
    public async Task<ListResponse<UserListDto>> GetUsersAsync(GetUserListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Get user according to user id.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Detail)]
    [HttpGet("user")]
    public async Task<Response<UserDetailDto>> GetUserAsync([FromQuery] GetUserDetailQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Adds user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Create)]
    [HttpPost("user")]
    public async Task<Response<int>> AddUserAsync(CreateUserCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Updates user. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Update)]
    [HttpPut("user")]
    public async Task<Response<int>> UpdateUserAsync(UpdateUserCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Removes user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.UserManagement.Delete)]
    [HttpDelete("user")]
    public async Task<Response<int>> RemoveUserAsync([FromQuery] DeleteUserCommand request) => await _mediator.Send(request);
}