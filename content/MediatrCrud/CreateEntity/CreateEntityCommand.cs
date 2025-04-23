using Milvasoft.Components.CQRS.Command;

namespace projectName.Application.Features.pluralName.CreateEntity;

/// <summary>
/// Data transfer object for entity creation.
/// </summary>
public record CreateEntityCommand : ICommand<int>
{
    /// <summary>
    /// Name of entity.
    /// </summary>
    public string Name { get; set; }
}