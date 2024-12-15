using Milvasoft.Components.CQRS.Query;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentList;

/// <summary>
/// Data transfer object for content list.
/// </summary>
public record GetContentListQuery : ListRequest, IListRequestQuery<ContentListDto>
{
}