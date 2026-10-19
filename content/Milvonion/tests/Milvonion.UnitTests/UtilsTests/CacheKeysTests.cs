using FluentAssertions;
using Milvonion.Application.Utils.Helpers;
using Milvonion.Application.Utils.Models;

namespace Milvonion.UnitTests.UtilsTests;

[Trait("Utils Unit Tests", "Redis key builder.")]
public class CacheKeysTests
{
    [Fact]
    public void EveryKey_ShouldStartWithTheApplicationPrefix()
    {
        // Arrange
        var cacheKeys = new CacheKeys(new CacheKeyOptions { Prefix = "shop-api" });

        // Act
        string[] keys = [cacheKeys.SecurityStamp(42), cacheKeys.RateLimit("account-login", "ip:10.0.0.1", 7), cacheKeys.DisabledFeatureFlags(), cacheKeys.Build("custom")];

        // Assert
        keys.Should().Equal("shop-api:sstamp:42", "shop-api:rl:account-login:ip:10.0.0.1:7", "shop-api:feature-flags-disabled", "shop-api:custom");
    }

    [Fact]
    public void SameEntry_OfTwoApplications_ShouldNotShareAKey()
    {
        // Arrange: an API and its admin API on one Redis, both with a user 42.
        var api = new CacheKeys(new CacheKeyOptions { Prefix = "shop-api" });
        var hub = new CacheKeys(new CacheKeyOptions { Prefix = "shop-hub" });

        // Act
        var apiKeys = new[] { api.SecurityStamp(42), api.DisabledFeatureFlags() };
        var hubKeys = new[] { hub.SecurityStamp(42), hub.DisabledFeatureFlags() };

        // Assert
        apiKeys.Should().NotIntersectWith(hubKeys);
    }

    [Fact]
    public void DefaultPrefix_ShouldBeTheProjectName()
    {
        // Act
        var options = new CacheKeyOptions();

        // Assert: the template writes the project name (lower case) into the default.
        options.Prefix.Should().Be(typeof(CacheKeyOptions).Assembly.GetName().Name.Split('.')[0].ToLowerInvariant());
    }
}
