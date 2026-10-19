using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Builder;
using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Core.Abstractions;
using Milvasoft.DataAccess.EfCore;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Ef;
using Milvasoft.JobScheduling;
using Milvonion.Application;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Aspects.UserActivityLogAspect;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Application.Utils.Helpers;
using Milvonion.Application.Utils.Models;
using Milvonion.Domain;
using Milvonion.Infrastructure.Jobs;
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
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ISecurityStampStore, SecurityStampStore>();
        services.AddScoped<IPermissionManager, PermissionManager>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IUIService, UIService>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        // Retry safety for the create endpoints marked [Idempotent]; see IdempotencyStore. Expired keys are deleted by IdempotencyKeyCleanerJob.
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();

        // Must be registered before AddMilvaInterception: interception decorates only the IInterceptable
        // implementations registered up to that call, and the transaction comes from that decoration.
        services.AddScoped<ITransactionRunner, TransactionRunner>();

        services.AddTransient(typeof(Lazy<>), typeof(MilvonionLazy<>));

        services.AddInternalJobs();

        services.AddDataAccessServices(configurationManager);

        // Redis is the shared state of every instance: the [Cache] interceptor, the token revocation stamps and the
        // rate limit counters. The connection is lazy and retries in the background, so the application starts
        // (and every Redis consumer falls back or fails open) while Redis is unreachable.
        services.AddMilvaCaching(configurationManager)
                .WithRedisAccessor();

        // Every Redis key of this application starts with its own prefix (CacheKeys), so applications sharing one Redis
        // never read each other's entries. Milvasoft's Redis options have no key prefix of their own.
        var cacheKeyOptions = configurationManager.GetSection(CacheKeyOptions.SectionName).Get<CacheKeyOptions>() ?? new CacheKeyOptions();

        services.AddSingleton(cacheKeyOptions);
        services.AddSingleton<CacheKeys>();

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
                    opt.Cache.CacheAccessorType = typeof(IRedisAccessor);

                    // The [Cache] keys are hashes of the method and its arguments: the prefix goes into the hash.
                    opt.Cache.CacheKeyConfigurator = _ => cacheKeyOptions.Prefix;
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

            options.UseNpgsql(dataSource, b => b.MigrationsHistoryTable(TableNames.EfMigrationHistory).MigrationsAssembly("Milvonion.Api").EnableRetryOnFailure())
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        });

        services.AddScoped<MilvonionDbContextScopedFactory>();
        services.AddScoped(sp => sp.GetRequiredService<MilvonionDbContextScopedFactory>().CreateDbContext());

        services.AddScoped(typeof(IMilvonionRepositoryBase<>), typeof(MilvonionRepositoryBase<>));
        services.AddScoped<IMilvonionDbContextAccessor, MilvonionDbContextAccessor>();

        return services;
    }

    /// <summary>
    /// Adds internal cron jobs.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddInternalJobs(this IServiceCollection services)
    {
        services.AddMilvaCronJob<ActivityLogCleanerJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = @"0 2 */30 * *"; // At 02:00 AM, every 30 days
            c.CronFormat = CronFormat.Standard;
        });

        services.AddMilvaCronJob<NotificationCleanerJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = @"0 3 */5 * *"; // At 03:00 AM, every 5 days
            c.CronFormat = CronFormat.Standard;
        });

        services.AddMilvaCronJob<IdempotencyKeyCleanerJob>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = @"0 * * * *"; // At minute 0 of every hour
            c.CronFormat = CronFormat.Standard;
        });

        return services;
    }
}
