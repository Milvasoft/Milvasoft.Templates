using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ContentManagementDtos.ResourceGroupDtos;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupList;

/// <summary>
/// Data transfer object for resource group list.
/// </summary>
public record GetResourceGroupListQuery : ListRequest, IListRequestQuery<ResourceGroupListDto>
{
}