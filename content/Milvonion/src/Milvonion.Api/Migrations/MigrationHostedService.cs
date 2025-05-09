﻿using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Infrastructure.Persistence.Context;
using Serilog;

namespace Milvonion.Api.Migrations;

/// <summary>
/// Applies migrations when the application starts.
/// </summary>
/// <param name="scopeFactory"></param>
public class MigrationHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    readonly IServiceScopeFactory _scopeFactory = scopeFactory;

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
            var languages = await context.Languages.ToListAsync(cancellationToken);

            var languageSeed = languages.Cast<ILanguage>().ToList();

            MultiLanguageManager.UpdateLanguagesList(languageSeed);

            var permissionManager = scope.ServiceProvider.GetRequiredService<IPermissionManager>();

            var permissions = await permissionManager.GetAllPermissionsAsync(cancellationToken);

            foreach (var permission in permissions)
                PermissionCatalog.Permissions.Add(permission);

        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while initializing language list.");
        }

        try
        {
            Log.Logger.Information("Applying migrations..");

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (!pendingMigrations.IsNullOrEmpty())
                await context.Database.MigrateAsync(cancellationToken);

            var languages = await context.Languages.ToListAsync(cancellationToken);

            var languageSeed = languages.Cast<ILanguage>().ToList();

            MultiLanguageManager.UpdateLanguagesList(languageSeed);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An error occurred while applying migrations.");
        }
    }

    /// <summary>
    /// Stops hosted service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}