using Milvasoft.Components.CQRS.Command;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateBulkContent;

/// <summary>
/// Data transfer object for contents creation.
/// </summary>
public record CreateBulkContentCommand : ICommand
{
    /// <summary>
    /// List of content creation objects.
    /// </summary>
    public List<CreateContentDto> Contents { get; init; }
}
