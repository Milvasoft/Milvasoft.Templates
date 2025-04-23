using Milvasoft.Components.CQRS.Command;
using Milvasoft.Types.Structs;

namespace projectName.Application.Features.pluralName.UpdateEntity;

/// <summary>
/// Data transfer object for entity update.
/// </summary>
public class UpdateEntityCommand : projectNameBaseDto<int>, ICommand<int>
{
    /// <summary>
    /// Name of entity.
    /// </summary>
    public UpdateProperty<string> Name { get; set; }
}
