using Microsoft.Extensions.DependencyInjection;
using Milvasoft.JobScheduling;

namespace Milvonion.Infrastructure.Jobs;

/// <summary>
/// Deletes the idempotency keys whose retry window has passed (<c>idempotencyKey_cleanup.sql</c>). A key only exists to
/// turn a retried create request into a replay of the first answer; a client never retries a day later, so older rows are
/// disposable and the table stays small. Runs hourly, so one run only ever deletes about an hour of keys.
/// </summary>
/// <param name="scheduleConfig"></param>
/// <param name="scopeFactory"></param>
public class IdempotencyKeyCleanerJob(IScheduleConfig scheduleConfig, IServiceScopeFactory scopeFactory) : SqlCleanupJob(scheduleConfig, scopeFactory)
{
    /// <inheritdoc/>
    protected override string SqlFileName => "idempotencyKey_cleanup.sql";
}
