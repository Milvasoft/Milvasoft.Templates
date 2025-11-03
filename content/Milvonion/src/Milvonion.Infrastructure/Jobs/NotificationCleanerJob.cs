using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.JobScheduling;
using Milvonion.Application.Utils.Constants;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Jobs;

/// <summary>
/// Truncate the expired notifications that are older than the specified date.
/// </summary>
/// <param name="scheduleConfig"></param>
/// <param name="scopeFactory"></param>
public class NotificationCleanerJob(IScheduleConfig scheduleConfig, IServiceScopeFactory scopeFactory) : MilvaCronJobService(scheduleConfig)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<IMilvaLogger>();

        try
        {
            var dbContext = scope.ServiceProvider.GetService<MilvonionDbContext>();

            var cleanupSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "notification_cleanup.sql"), cancellationToken);

            var affectedRows = await dbContext.Database.ExecuteSqlRawAsync(cleanupSql, cancellationToken);

            logger.Information(LogTemplate.JobExecuted, nameof(NotificationCleanerJob), affectedRows);
        }
        catch (Exception ex)
        {
            logger.Error(ex, LogTemplate.JobException, nameof(NotificationCleanerJob), ex.Message);
        }
    }
}