using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.Infrastructure.Jobs;
using Milvonion.IntegrationTests.TestBase;

namespace Milvonion.IntegrationTests.ServicesTests;

/// <summary>
/// The cleanup jobs log and swallow their own errors (a wrong column name in a SQL file only shows up in the log), so
/// every job is run here against the real database and the test asserts which rows are gone and which are kept.
/// </summary>
/// <param name="factory"></param>
[Collection(nameof(MilvonionApiCollection))]
[Trait("Service Integration Tests", "Integration tests for the cleanup jobs.")]
public class CleanupJobsTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task IdempotencyKeyCleanerJob_ShouldDeleteOnlyTheKeysOlderThanTheRetryWindow()
    {
        // Arrange: one key past the 24 hour window, one fresh key.
        var expiredKey = Guid.NewGuid().ToString("N");
        var freshKey = Guid.NewGuid().ToString("N");

        await _factory.DbAsync(async db =>
        {
            await db.IdempotencyKeys.AddRangeAsync([
                new IdempotencyKey { UserId = 1, Key = expiredKey, Endpoint = "POST test", RequestHash = new byte[32], CreationDate = DateTime.UtcNow.AddHours(-25) },
                new IdempotencyKey { UserId = 1, Key = freshKey, Endpoint = "POST test", RequestHash = new byte[32], CreationDate = DateTime.UtcNow }
            ], TestContext.Current.CancellationToken);

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

        // Act
        await RunJobAsync<IdempotencyKeyCleanerJob>();

        // Assert
        var remainingKeys = await _factory.DbAsync(db => db.IdempotencyKeys.Where(k => k.Key == expiredKey || k.Key == freshKey).Select(k => k.Key).ToListAsync(TestContext.Current.CancellationToken));

        remainingKeys.Should().Equal(freshKey);
    }

    [Fact]
    public async Task ActivityLogCleanerJob_ShouldDeleteOnlyTheLogsOlderThanSixtyDays()
    {
        // Arrange: one log past the 60 day window, one recent log, both under a user name no other test uses.
        var userName = $"cleanup{TestApi.Unique()}";

        await _factory.DbAsync(async db =>
        {
            await db.ActivityLogs.AddRangeAsync([
                new ActivityLog { UserName = userName, Activity = UserActivity.CreateUser, ActivityDate = DateTimeOffset.UtcNow.AddDays(-61), RelatedId = "expired" },
                new ActivityLog { UserName = userName, Activity = UserActivity.CreateUser, ActivityDate = DateTimeOffset.UtcNow.AddDays(-1), RelatedId = "recent" }
            ], TestContext.Current.CancellationToken);

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

        // Act
        await RunJobAsync<ActivityLogCleanerJob>();

        // Assert
        var remainingLogs = await _factory.DbAsync(db => db.ActivityLogs.Where(l => l.UserName == userName).Select(l => l.RelatedId).ToListAsync(TestContext.Current.CancellationToken));

        remainingLogs.Should().Equal("recent");
    }

    [Fact]
    public async Task NotificationCleanerJob_ShouldDeleteTheOldSeenAndTheExpiredUnseenNotifications()
    {
        // Arrange: seen 31 days ago (deleted), seen yesterday (kept), unseen and created 61 days ago (deleted), unseen and
        // created yesterday (kept). The creation date is audited on insert, so it is moved back with an update afterwards.
        var recipient = $"cleanup{TestApi.Unique()}";

        await _factory.DbAsync(async db =>
        {
            await db.InternalNotifications.AddRangeAsync([
                new InternalNotification { RecipientUserName = recipient, RecipientUserId = 1, Type = NotificationType.OrderApproved, Text = "seen-old", SeenDate = DateTime.UtcNow.AddDays(-31) },
                new InternalNotification { RecipientUserName = recipient, RecipientUserId = 1, Type = NotificationType.OrderApproved, Text = "seen-recent", SeenDate = DateTime.UtcNow.AddDays(-1) },
                new InternalNotification { RecipientUserName = recipient, RecipientUserId = 1, Type = NotificationType.OrderApproved, Text = "unseen-old" },
                new InternalNotification { RecipientUserName = recipient, RecipientUserId = 1, Type = NotificationType.OrderApproved, Text = "unseen-recent" }
            ], TestContext.Current.CancellationToken);

            await db.SaveChangesAsync(TestContext.Current.CancellationToken);

            await db.InternalNotifications.Where(n => n.RecipientUserName == recipient && n.Text == "unseen-old")
                                          .ExecuteUpdateAsync(s => s.SetProperty(n => n.CreationDate, DateTime.UtcNow.AddDays(-61)), TestContext.Current.CancellationToken);

            await db.InternalNotifications.Where(n => n.RecipientUserName == recipient && n.Text == "unseen-recent")
                                          .ExecuteUpdateAsync(s => s.SetProperty(n => n.CreationDate, DateTime.UtcNow.AddDays(-1)), TestContext.Current.CancellationToken);
        });

        // Act
        await RunJobAsync<NotificationCleanerJob>();

        // Assert
        var remainingNotifications = await _factory.DbAsync(db => db.InternalNotifications.Where(n => n.RecipientUserName == recipient)
                                                                                           .OrderBy(n => n.Text)
                                                                                           .Select(n => n.Text)
                                                                                           .ToListAsync(TestContext.Current.CancellationToken));

        remainingNotifications.Should().Equal("seen-recent", "unseen-recent");
    }

    /// <summary>
    /// Runs a cleanup job now instead of waiting for its schedule (AddInternalJobs registers it as a hosted service).
    /// </summary>
    private async Task RunJobAsync<TJob>() where TJob : SqlCleanupJob
    {
        var job = _factory.Services.GetServices<IHostedService>().OfType<TJob>().Single();

        await job.ExecuteAsync(TestContext.Current.CancellationToken);
    }
}
