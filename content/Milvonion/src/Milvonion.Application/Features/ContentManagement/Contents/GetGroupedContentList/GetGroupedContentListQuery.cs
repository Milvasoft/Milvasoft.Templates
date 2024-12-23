using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetGroupedContentList;

/// <summary>
/// Data transfer object for content list.
/// </summary>
public record GetGroupedContentListQuery : ListRequest, IListRequestQuery<GroupedContentListDto>
{
}