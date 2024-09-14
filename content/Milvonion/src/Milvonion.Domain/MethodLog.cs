using Milvasoft.Core.EntityBases.Concrete;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the MethodLogs table.
/// </summary>
[Table(TableNames.MethodLogs)]
public class MethodLog : LogEntityBase<int>
{

}
