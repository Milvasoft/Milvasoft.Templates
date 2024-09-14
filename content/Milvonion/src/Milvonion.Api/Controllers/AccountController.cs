using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Features.Account.AccountDetail;
using Milvonion.Application.Features.Account.ChangePassword;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Features.Account.Logout;
using Milvonion.Application.Features.Account.RefreshLogin;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Account endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
public class AccountController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// User login operation.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Token information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<Response<LoginResponseDto>> LoginAsync(LoginCommand request) => await _mediator.Send(request);

    /// <summary>
    /// User refresh login operation.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login/refresh")]
    [AllowAnonymous]
    public async Task<Response<LoginResponseDto>> RefreshLoginAsync(RefreshLoginCommand request) => await _mediator.Send(request);

    /// <summary>
    /// User logout operation.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    [HttpPost("logout")]
    public async Task<Response> LogoutAsync(LogoutCommand request) => await _mediator.Send(request);

    /// <summary>
    /// User's own password change operation.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth]
    [HttpPut("password/change")]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    public async Task<Response> ChangePasswordAsync(ChangePasswordCommand request) => await _mediator.Send(request);

    /// <summary>
    /// User can access his/her account information through this endpoint. If the logged in user and the sent id information do not match, the request will fail.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("detail")]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    public async Task<Response<AccountDetailDto>> AccountDetailsAsync([FromQuery] AccountDetailQuery request) => await _mediator.Send(request);
}