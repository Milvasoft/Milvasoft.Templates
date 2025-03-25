using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.UIDtos;
using Milvonion.Application.Dtos.UIDtos.MenuItemDtos;
using Milvonion.Application.Dtos.UIDtos.PageDtos;
using Milvonion.Application.Features.Pages.GetPageAccessibilityForCurrentUser;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Frontend related endpoints.
/// </summary>
[Auth]
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
public class UIController(IMediator mediator, IUIService uiService) : ControllerBase
{

    /// <summary>
    /// Gets accessible menu items.
    /// </summary>
    /// <returns></returns>
    [HttpGet("menuItems")]
    public Task<Response<List<MenuItemDto>>> GetAccessibleMenuItemsAsync(CancellationToken cancellation) => uiService.GetAccessibleMenuItemsForCurrentUserAsync(cancellation);

    /// <summary>
    /// Gets page information of current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("pages/page")]
    public Task<Response<PageDto>> GetPageAccessibilityForCurrentUserAsync([FromQuery] GetPageAccessibilityForCurrentUserQuery reqeust, CancellationToken cancellation) => mediator.Send(reqeust, cancellation);

    /// <summary>
    /// Gets page information of current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("pages")]
    public Task<Response<List<PageDto>>> GetPageAccessibilityForCurrentUserAsync(CancellationToken cancellation) => uiService.GetCurrentUserPagesAccessibilityAsync(cancellation);

    /// <summary>
    /// Gets localized contents related with UI.
    /// </summary>
    /// <returns></returns>
    [HttpGet("localizedContents")]
    [AllowAnonymous]
    public Response<List<LocalizedContentDto>> GetLocalizedContents() => uiService.GetLocalizedContents();
}
