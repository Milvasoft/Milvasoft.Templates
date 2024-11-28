using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.EntityBases.Concrete;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the MethodLogs table.
/// </summary>
[Table(TableNames.MethodLogs)]
[Index(nameof(UtcLogTime), IsDescending = [true])]
[Index(nameof(IsSuccess))]
[Index(nameof(TransactionId))]
public class MethodLog : LogEntityBase<int>
{

}
