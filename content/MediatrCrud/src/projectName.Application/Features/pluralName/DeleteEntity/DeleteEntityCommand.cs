using Milvasoft.Components.CQRS.Command;

namespace projectName.Application.Features.pluralName.DeleteEntity;

/// <summary>
/// Data transfer object for entity deletion.
/// </summary>
public record DeleteEntityCommand : ICommand<datatypefe>
{
    /// <summary>
    /// Id of the entity to be deleted.
    /// </summary>
    public datatypefe EntityId { get; set; }
}
