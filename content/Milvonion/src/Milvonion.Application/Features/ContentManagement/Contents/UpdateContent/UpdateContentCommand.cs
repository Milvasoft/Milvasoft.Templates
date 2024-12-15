using Milvasoft.Components.CQRS.Command;
using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Types.Structs;
using Milvonion.Application.Dtos.ContentManagementDtos.ContentDtos;

namespace Milvonion.Application.Features.ContentManagement.Contents.UpdateContent;

/// <summary>
/// Data transfer object for content update.
/// </summary>
public class UpdateContentCommand : BaseDto<int>, ICommand<int>
{
    /// <summary>
    /// Name of content.
    /// </summary>
    public UpdateProperty<string> Value { get; set; }

    /// <summary>
    /// Medias of content.
    /// </summary>
    public UpdateProperty<List<UpsertMediaDto>> Medias { get; set; }
}
