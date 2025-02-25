using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.InMemory;
using Milvasoft.Core.Abstractions;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Ef;
using Milvonion.Application;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Aspects.UserActivityLogAspect;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Domain;
using Milvonion.Infrastructure.LazyImpl;
using Milvonion.Infrastructure.Logging;
using Milvonion.Infrastructure.Persistence.Context;
using Milvonion.Infrastructure.Persistence.Repository;
using Milvonion.Infrastructure.Services;
using Npgsql;

namespace Milvonion.Infrastructure;

/// <summary>
/// Infrastructure service collection extensions.
/// </summary>
public static class InfraServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        services.AddScoped<IMilvaLogger, MilvonionDbLogger>();
        services.AddScoped<IPermissionManager, PermissionManager>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IUIService, UIService>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IExportService, ExportService>();

        services.AddTransient(typeof(Lazy<>), typeof(MilvonionLazy<>));

        services.AddDataAccessServices(configurationManager);

        services.AddMilvaCaching(configurationManager)
                .WithInMemoryAccessor();

        services.AddMilvaInterception(ApplicationAssembly.Assembly, configurationManager)
                .WithLogInterceptor()
                .WithResponseInterceptor()
                .WithCacheInterceptor()
                .WithTransactionInterceptor()
                .WithActivityInterceptor()
                .WithInterceptor<UserActivityLogInterceptor>()
                .PostConfigureInterceptionOptions(opt =>
                {
                    opt.Response.GenerateMetadataFunc = MilvonionExtensions.GenerateMetadata;
                })
                .PostConfigureTransactionInterceptionOptions(opt =>
                {
                    opt.DbContextType = typeof(MilvonionDbContext);
                });

        return services;
    }

    /// <summary>
    /// Adds data access services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationManager"></param>
    /// <returns></returns>
    private static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        var connectionString = configurationManager.GetConnectionString("DefaultConnectionString");

        services.ConfigureMilvaDataAccess(configurationManager)
                .PostConfigureMilvaDataAccess(opt =>
                {
                    opt.GetCurrentUserNameMethod = User.GetCurrentUser;
                })
                .AddInjectedDbContext<MilvonionDbContext>();

        var dataSource = new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson().Build();

        services.AddPooledDbContextFactory<MilvonionDbContext>((provider, options) =>
        {
            options.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });

            options.UseNpgsql(dataSource, b => b.MigrationsHistoryTable(TableNames.MigrationHistory).MigrationsAssembly("Milvonion.Api").EnableRetryOnFailure())
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        services.AddScoped<MilvonionDbContextScopedFactory>();
        services.AddScoped(sp => sp.GetRequiredService<MilvonionDbContextScopedFactory>().CreateDbContext());

        services.AddScoped(typeof(IMilvonionRepositoryBase<>), typeof(MilvonionRepositoryBase<>));
        services.AddScoped<IMilvonionDbContextAccessor, MilvonionDbContextAccessor>();

        return services;
    }
}
