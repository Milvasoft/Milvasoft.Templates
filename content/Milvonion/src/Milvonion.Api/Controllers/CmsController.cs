using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;
using Milvonion.Application.Dtos.ContentManagementDtos.NamespaceDtos;
using Milvonion.Application.Dtos.ContentManagementDtos.ResourceGroupDtos;
using Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;
using Milvonion.Application.Features.ContentManagement.Contents.CreateContent;
using Milvonion.Application.Features.ContentManagement.Contents.DeleteContents;
using Milvonion.Application.Features.ContentManagement.Contents.GetContent;
using Milvonion.Application.Features.ContentManagement.Contents.GetContentDetail;
using Milvonion.Application.Features.ContentManagement.Contents.GetContentList;
using Milvonion.Application.Features.ContentManagement.Contents.GetGroupedContentList;
using Milvonion.Application.Features.ContentManagement.Contents.UpdateContent;
using Milvonion.Application.Features.ContentManagement.Namespaces.CreateNamespace;
using Milvonion.Application.Features.ContentManagement.Namespaces.DeleteNamespace;
using Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceDetail;
using Milvonion.Application.Features.ContentManagement.Namespaces.GetNamespaceList;
using Milvonion.Application.Features.ContentManagement.Namespaces.UpdateNamespace;
using Milvonion.Application.Features.ContentManagement.ResourceGroups.CreateResourceGroup;
using Milvonion.Application.Features.ContentManagement.ResourceGroups.DeleteResourceGroup;
using Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupDetail;
using Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupList;
using Milvonion.Application.Features.ContentManagement.ResourceGroups.UpdateResourceGroup;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Content endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[UserTypeAuth(UserType.Manager)]
public class CmsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    #region Namespace

    /// <summary>
    /// Gets namespaces.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.List)]
    [HttpPatch("namespaces")]
    public async Task<ListResponse<NamespaceListDto>> GetNamespacesAsync(GetNamespaceListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Get namespace according to namespace id.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Detail)]
    [HttpGet("namespaces/namespace")]
    public async Task<Response<NamespaceDetailDto>> GetNamespaceAsync([FromQuery] GetNamespaceDetailQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Adds namespace.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Create)]
    [HttpPost("namespaces/namespace")]
    public async Task<Response<int>> AddNamespaceAsync(CreateNamespaceCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Updates namespace. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Update)]
    [HttpPut("namespaces/namespace")]
    public async Task<Response<int>> UpdateNamespaceAsync(UpdateNamespaceCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Removes namespace.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Delete)]
    [HttpDelete("namespaces/namespace")]
    public async Task<Response<int>> RemoveNamespaceAsync([FromQuery] DeleteNamespaceCommand request) => await _mediator.Send(request);

    #endregion

    #region Resource Group

    /// <summary>
    /// Gets resource groups.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.List)]
    [HttpPatch("resourceGroups")]
    public async Task<ListResponse<ResourceGroupListDto>> GetResourceGroupsAsync(GetResourceGroupListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Get resource group according to resourceGroup id.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Detail)]
    [HttpGet("resourceGroups/resourceGroup")]
    public async Task<Response<ResourceGroupDetailDto>> GetResourceGroupAsync([FromQuery] GetResourceGroupDetailQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Adds resource group.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Create)]
    [HttpPost("resourceGroups/resourceGroup")]
    public async Task<Response<int>> AddResourceGroupAsync(CreateResourceGroupCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Updates resource group. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Update)]
    [HttpPut("resourceGroups/resourceGroup")]
    public async Task<Response<int>> UpdateResourceGroupAsync(UpdateResourceGroupCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Removes resource group.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Delete)]
    [HttpDelete("resourceGroups/resourceGroup")]
    public async Task<Response<int>> RemoveResourceGroupAsync([FromQuery] DeleteResourceGroupCommand request) => await _mediator.Send(request);

    #endregion

    #region Content

    /// <summary>
    /// Query contents.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPatch("contents/query")]
    public async Task<Response<List<ContentDto>>> GetContentAsync(GetContentQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Gets contents.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.List)]
    [HttpPatch("contents")]
    public async Task<ListResponse<ContentListDto>> GetContentsAsync(GetContentListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Gets contents as grouped by key.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.List)]
    [HttpPatch("contents/by/key")]
    public async Task<ListResponse<GroupedContentListDto>> GetGroupedContentsAsync(GetGroupedContentListQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Get content according to content id.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Detail)]
    [HttpGet("contents/content")]
    public async Task<Response<ContentDetailDto>> GetContentAsync([FromQuery] GetContentDetailQuery request) => await _mediator.Send(request);

    /// <summary>
    /// Adds content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Create)]
    [HttpPost("contents/content")]
    public async Task<Response<int>> AddContentAsync(CreateContentCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Adds contents with bulk method.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Create)]
    [HttpPost("contents")]
    public async Task<Response> AddContentsAsync(CreateBulkContentCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Updates content. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Update)]
    [HttpPut("contents/content")]
    public async Task<Response<int>> UpdateContentAsync(UpdateContentCommand request) => await _mediator.Send(request);

    /// <summary>
    /// Removes content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Delete)]
    [HttpDelete("contents")]
    public async Task<Response<List<int>>> RemoveContentsAsync(DeleteContentsCommand request) => await _mediator.Send(request);

    #endregion
}