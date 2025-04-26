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
    /// <summary>
    /// Not validate token endpoint paths.
    /// </summary>
    public static List<string> LoginEndpointPaths { get; } = ["login", "refresh"];

    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// User login operation.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns>Token information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public Task<Response<LoginResponseDto>> LoginAsync(LoginCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// User refresh login operation.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpPost("login/refresh")]
    [AllowAnonymous]
    public Task<Response<LoginResponseDto>> RefreshLoginAsync(RefreshLoginCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// User logout operation.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    [HttpPost("logout")]
    public Task<Response> LogoutAsync(LogoutCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// User's own password change operation.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth]
    [HttpPut("password/change")]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    public Task<Response> ChangePasswordAsync(ChangePasswordCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// User can access his/her account information through this endpoint. If the logged in user and the sent id information do not match, the request will fail.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth]
    [HttpGet("detail")]
    [UserTypeAuth(UserType.Manager | UserType.AppUser)]
    public Task<Response<AccountDetailDto>> AccountDetailsAsync([FromQuery] AccountDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);
}