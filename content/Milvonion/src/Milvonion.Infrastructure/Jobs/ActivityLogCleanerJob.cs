using Microsoft.Extensions.DependencyInjection;
using Milvasoft.JobScheduling;

namespace Milvonion.Infrastructure.Jobs;

/// <summary>
/// Deletes the activity logs older than 60 days (<c>activityLog_cleanup.sql</c>).
/// </summary>
/// <param name="scheduleConfig"></param>
/// <param name="scopeFactory"></param>
public class ActivityLogCleanerJob(IScheduleConfig scheduleConfig, IServiceScopeFactory scopeFactory) : SqlCleanupJob(scheduleConfig, scopeFactory)
{
    /// <inheritdoc/>
    protected override string SqlFileName => "activityLog_cleanup.sql";
}
