using Milvasoft.Components.CQRS.Command;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.CreateContent;

/// <summary>
/// Data transfer object for content creation.
/// </summary>
public record CreateContentCommand : CreateContentDto, ICommand<int>
{
}