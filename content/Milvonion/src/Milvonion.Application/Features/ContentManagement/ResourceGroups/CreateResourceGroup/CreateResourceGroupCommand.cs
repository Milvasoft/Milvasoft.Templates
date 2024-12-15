using Milvasoft.Components.CQRS.Command;

namespace Milvonion.Application.Features.ContentManagement.ResourceGroups.CreateResourceGroup;

/// <summary>
/// Data transfer object for resource group creation.
/// </summary>
public record CreateResourceGroupCommand : ICommand<int>
{
    /// <summary>
    /// Belongs to which namespace.
    /// </summary>
    public int NamespaceId { get; set; }

    /// <summary>
    /// Name of resource group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of resource group.
    /// </summary>
    public string Description { get; set; }
}