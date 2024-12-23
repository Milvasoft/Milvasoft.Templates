using Milvasoft.DataAccess.EfCore.Bulk;
using Milvonion.Application.Interfaces;

namespace Milvonion.Infrastructure.Persistence.Context;

/// <summary>
/// Milvonion scoped factory.
/// </summary>
/// <remarks>
/// Initializes new instance of <see cref="MilvonionDbContextScopedFactory"/>.
/// </remarks>
/// <param name="context"></param>
public class MilvonionDbContextAccessor(MilvonionDbContext context) : IMilvonionDbContextAccessor
{

    /// <summary>
    /// Db context creation implementation.
    /// </summary>
    /// <returns></returns>
    public IMilvaBulkDbContextBase GetDbContext() => context;
}
