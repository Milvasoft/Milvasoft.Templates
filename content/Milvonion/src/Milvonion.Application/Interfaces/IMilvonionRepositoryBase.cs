using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract;

namespace Milvonion.Application.Interfaces;

/// <summary>
/// Base repository for Milvonion.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IMilvonionRepositoryBase<TEntity> : IBaseRepository<TEntity> where TEntity : EntityBase
{
}