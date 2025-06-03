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
[ApiVersion(GlobalConstant.CurrentApiVersion)]
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
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.List)]
    [HttpPatch("namespaces")]
    public Task<ListResponse<NamespaceListDto>> GetNamespacesAsync(GetNamespaceListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Get namespace according to namespace id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Detail)]
    [HttpGet("namespaces/namespace")]
    public Task<Response<NamespaceDetailDto>> GetNamespaceAsync([FromQuery] GetNamespaceDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds namespace.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Create)]
    [HttpPost("namespaces/namespace")]
    public Task<Response<int>> AddNamespaceAsync(CreateNamespaceCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates namespace. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Update)]
    [HttpPut("namespaces/namespace")]
    public Task<Response<int>> UpdateNamespaceAsync(UpdateNamespaceCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Removes namespace.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.NamespaceManagement.Delete)]
    [HttpDelete("namespaces/namespace")]
    public Task<Response<int>> RemoveNamespaceAsync([FromQuery] DeleteNamespaceCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    #endregion

    #region Resource Group

    /// <summary>
    /// Gets resource groups.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.List)]
    [HttpPatch("resourceGroups")]
    public Task<ListResponse<ResourceGroupListDto>> GetResourceGroupsAsync(GetResourceGroupListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Get resource group according to resourceGroup id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Detail)]
    [HttpGet("resourceGroups/resourceGroup")]
    public Task<Response<ResourceGroupDetailDto>> GetResourceGroupAsync([FromQuery] GetResourceGroupDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds resource group.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Create)]
    [HttpPost("resourceGroups/resourceGroup")]
    public Task<Response<int>> AddResourceGroupAsync(CreateResourceGroupCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates resource group. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Update)]
    [HttpPut("resourceGroups/resourceGroup")]
    public Task<Response<int>> UpdateResourceGroupAsync(UpdateResourceGroupCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Removes resource group.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ResourceGroupManagement.Delete)]
    [HttpDelete("resourceGroups/resourceGroup")]
    public Task<Response<int>> RemoveResourceGroupAsync([FromQuery] DeleteResourceGroupCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    #endregion

    #region Content

    /// <summary>
    /// Query contents.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpPatch("contents/query")]
    public Task<Response<List<ContentDto>>> GetContentAsync(GetContentQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Gets contents.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.List)]
    [HttpPatch("contents")]
    public Task<ListResponse<ContentListDto>> GetContentsAsync(GetContentListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Gets contents as grouped by key.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.List)]
    [HttpPatch("contents/by/key")]
    public Task<ListResponse<GroupedContentListDto>> GetGroupedContentsAsync(GetGroupedContentListQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Get content according to content id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Detail)]
    [HttpGet("contents/content")]
    public Task<Response<ContentDetailDto>> GetContentDetailAsync([FromQuery] GetContentDetailQuery request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds content.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Create)]
    [HttpPost("contents/content")]
    public Task<Response<int>> AddContentAsync(CreateContentCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Adds contents with bulk method.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Create)]
    [HttpPost("contents")]
    public Task<Response> AddContentsAsync(CreateBulkContentCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Updates content. Only the fields that are sent as isUpdated true are updated.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Update)]
    [HttpPut("contents/content")]
    public Task<Response<int>> UpdateContentAsync(UpdateContentCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    /// <summary>
    /// Removes content.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [Auth(PermissionCatalog.ContentManagement.Delete)]
    [HttpDelete("contents")]
    public Task<Response<List<int>>> RemoveContentsAsync(DeleteContentsCommand request, CancellationToken cancellation) => _mediator.Send(request, cancellation);

    #endregion
}