﻿using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.Helpers;
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
        try
        {
            Log.Logger.Information("Applying migrations..");

            await using var scope = _scopeFactory.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<MilvonionDbContext>();

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (!pendingMigrations.IsNullOrEmpty())
                await context.Database.MigrateAsync(cancellationToken);
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