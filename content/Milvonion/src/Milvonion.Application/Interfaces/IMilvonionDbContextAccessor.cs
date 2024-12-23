using Milvasoft.DataAccess.EfCore.Bulk;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Interface for MilvonionDbContextAccessor.
/// </summary>
public interface IMilvonionDbContextAccessor
{
    /// <summary>
    /// Get db context.
    /// </summary>
    /// <returns></returns>
    IMilvaBulkDbContextBase GetDbContext();
}