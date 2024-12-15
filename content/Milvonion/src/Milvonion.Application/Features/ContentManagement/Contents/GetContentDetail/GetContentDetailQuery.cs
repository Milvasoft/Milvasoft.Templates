using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.GetContentDetail;

/// <summary>
/// Data transfer object for content details.
/// </summary>
public record GetContentDetailQuery : IQuery<ContentDetailDto>
{
    /// <summary>
    /// Resource group id to access details.
    /// </summary>
    public int ContentId { get; set; }
}
