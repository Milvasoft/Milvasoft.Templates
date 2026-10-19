using Milvasoft.Core.EntityBases.Concrete;

namespace Milvonion.Domain;

/// <summary>
/// Shape of a method log entry produced by the <c>[Log]</c> interceptor. It is not a table: <c>MilvonionDbLogger</c>
/// deserializes the entry into it and writes it to the log pipeline (Serilog), never to the application database.
/// </summary>
public class MethodLog : LogEntityBase<int>
{

}