using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.ContentManagementDtos.ResourceGroupDtos;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.GetResourceGroupDetail;

/// <summary>
/// Data transfer object for resource group details.
/// </summary>
public record GetResourceGroupDetailQuery : IQuery<ResourceGroupDetailDto>
{
    /// <summary>
    /// Resource group id to access details.
    /// </summary>
    public int ResourceGroupId { get; set; }
}
