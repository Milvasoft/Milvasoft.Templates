using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.JobScheduling;
using Milvonion.Application.Utils.Constants;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Jobs;

/// <summary>
/// Base of the cleanup jobs: on every run executes one SQL file of <see cref="GlobalConstant.SqlFilesPath"/> against the
/// application database and logs the affected row count. A failure is logged and swallowed, so a cleanup problem never
/// stops the host; the next run retries. Because the job hides its own errors, every derived job has an integration test
/// that runs it against a real database and asserts that the expected rows are gone.
/// </summary>
/// <param name="scheduleConfig"></param>
/// <param name="scopeFactory"></param>
public abstract class SqlCleanupJob(IScheduleConfig scheduleConfig, IServiceScopeFactory scopeFactory) : MilvaCronJobService(scheduleConfig)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <summary>
    /// Name of the SQL file (under <see cref="GlobalConstant.SqlFilesPath"/>) the job executes.
    /// </summary>
    protected abstract string SqlFileName { get; }

    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<IMilvaLogger>();

        var jobName = GetType().Name;

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MilvonionDbContext>();

            var cleanupSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, SqlFileName), cancellationToken);

            var affectedRows = await dbContext.Database.ExecuteSqlRawAsync(cleanupSql, cancellationToken);

            logger.Information(LogTemplate.JobExecuted, jobName, affectedRows);
        }
        catch (Exception ex)
        {
            logger.Error(ex, LogTemplate.JobException, jobName, ex.Message);
        }
    }
}
