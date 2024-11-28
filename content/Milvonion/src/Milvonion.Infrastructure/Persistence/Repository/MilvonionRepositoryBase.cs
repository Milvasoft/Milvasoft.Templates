using Milvasoft.Core.EntityBases.Concrete;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Milvonion.Application.Interfaces;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Persistence.Repository;

/// <summary>
/// Constructor of <c>BillRepository</c> class.
/// </summary>
/// <param name="dbContext"></param>
public class MilvonionRepositoryBase<TEntity>(MilvonionDbContext dbContext) : BulkBaseRepository<TEntity, MilvonionDbContext>(dbContext), IMilvonionRepositoryBase<TEntity>
    where TEntity : EntityBase;
