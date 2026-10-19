using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.Helpers;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Infrastructure.Persistence;
using Milvonion.Infrastructure.Persistence.Context;
using Serilog;

namespace Milvonion.Api.Migrations;

/// <summary>
/// Applies migrations when the application starts, creates the missing feature flag rows, then loads the language and
/// permission lists the app reads from memory. Every step is idempotent, so it is safe on every startup.
/// </summary>
/// <param name="scopeFactory"></param>
public class MigrationHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <summary>
    /// Starts hosted service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<MilvonionDbContext>();

        try
        {
            Log.Logger.Information("Applying migrations..");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (!pendingMigrations.IsNullOrEmpty())
            {
                Log.Logger.Information("Applying {Count} pending migration(s)...", pendingMigrations.Count());

                await context.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while applying migrations.");
        }

        try
        {
            // Every switch in FeatureFlagKeys gets its row, so it can be toggled from the management endpoints.
            await FeatureFlagSeeder.SeedAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while creating the feature flags.");
        }

        try
        {
            // Runs after the migration, so a fresh database already has the tables.
            await LanguageSeeder.LoadAsync(context, cancellationToken);

            if (PermissionCatalog.Permissions.IsEmpty)
            {
                var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();

                var permissions = await permissionManager.GetAllPermissionsAsync(cancellationToken);

                foreach (var permission in permissions)
                    PermissionCatalog.Permissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while initializing language and permission lists.");
        }
    }

    /// <summary>
    /// Stops hosted service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}