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
    private readonly IDbContextFactory<MilvonionDbContext> _pooledFactory = pooledFactory;
    private readonly IDataAccessConfiguration _dataAccessConfiguration = dataAccessConfiguration;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Db context creation implementation.
    /// </summary>
    /// <returns></returns>
    public MilvonionDbContext CreateDbContext()
    {
        var context = _pooledFactory.CreateDbContext();

        context.ServiceProvider = _serviceProvider;
        context.SetDataAccessConfiguration(_dataAccessConfiguration);

        return context;
    }
}
