using Microsoft.Extensions.DependencyInjection;
using Milvasoft.JobScheduling;

namespace Milvonion.Infrastructure.Jobs;

/// <summary>
/// Deletes the internal notifications seen more than 30 days ago and the unseen ones created more than 60 days ago
/// (<c>notification_cleanup.sql</c>).
/// </summary>
/// <param name="scheduleConfig"></param>
/// <param name="scopeFactory"></param>
public class NotificationCleanerJob(IScheduleConfig scheduleConfig, IServiceScopeFactory scopeFactory) : SqlCleanupJob(scheduleConfig, scopeFactory)
{
    /// <inheritdoc/>
    protected override string SqlFileName => "notification_cleanup.sql";
}
