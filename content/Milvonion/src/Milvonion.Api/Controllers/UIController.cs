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
    public async Task<Response<List<MenuItemDto>>> GetAccessibleMenuItemsAsync() => await uiService.GetAccessibleMenuItemsForCurrentUserAsync();

    /// <summary>
    /// Gets page information of current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("pages/page")]
    public async Task<Response<PageDto>> GetPageAccessibilityForCurrentUserAsync([FromQuery] GetPageAccessibilityForCurrentUserQuery reqeust) => await mediator.Send(reqeust);

    /// <summary>
    /// Gets page information of current user.
    /// </summary>
    /// <returns></returns>
    [HttpGet("pages")]
    public async Task<Response<List<PageDto>>> GetPageAccessibilityForCurrentUserAsync() => await uiService.GetCurrentUserPagesAccessibilityAsync();

    /// <summary>
    /// Gets localized contents related with UI.
    /// </summary>
    /// <returns></returns>
    [HttpGet("localizedContents")]
    [AllowAnonymous]
    public Response<List<LocalizedContentDto>> GetLocalizedContents() => uiService.GetLocalizedContents();
}
