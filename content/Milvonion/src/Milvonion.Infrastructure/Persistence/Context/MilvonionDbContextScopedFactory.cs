using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Configuration;

namespace Milvonion.Infrastructure.Persistence.Context;

/// <summary>
/// Milvonion scoped factory.
/// </summary>
/// <remarks>
/// Initializes new instance of <see cref="MilvonionDbContextScopedFactory"/>.
/// </remarks>
/// <param name="pooledFactory"></param>
/// <param name="dataAccessConfiguration"></param>
/// <param name="serviceProvider"></param>
public class MilvonionDbContextScopedFactory(IDbContextFactory<MilvonionDbContext> pooledFactory,
                                           IDataAccessConfiguration dataAccessConfiguration,
                                           IServiceProvider serviceProvider) : IDbContextFactory<MilvonionDbContext>
{

    /// <summary>
    /// Db context creation implementation.
    /// </summary>
    /// <returns></returns>
    public MilvonionDbContext CreateDbContext()
    {
        var context = pooledFactory.CreateDbContext();

        context.ServiceProvider = serviceProvider;
        context.SetDataAccessConfiguration(dataAccessConfiguration);

        return context;
    }
}
