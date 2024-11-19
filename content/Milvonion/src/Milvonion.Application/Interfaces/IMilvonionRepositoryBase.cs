using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.DataAccess.EfCore.Bulk.RepositoryBase.Abstract;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Base repository for Milvonion.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IMilvonionRepositoryBase<TEntity> : IBulkBaseRepository<TEntity> where TEntity : EntityBase
{
}