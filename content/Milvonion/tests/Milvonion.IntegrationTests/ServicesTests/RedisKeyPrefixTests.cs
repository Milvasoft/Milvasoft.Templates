using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvonion.Application.Utils.Models;
using Milvonion.IntegrationTests.TestBase;
using StackExchange.Redis;
using System.Net;

namespace Milvonion.IntegrationTests.ServicesTests;

/// <summary>
/// Two applications that share one Redis (an API and its admin API, two environments on one server) must not read each
/// other's revocation stamps, rate limit counters or feature flag cache: every key starts with the application's prefix.
/// </summary>
/// <param name="factory"></param>
[Collection(nameof(MilvonionApiCollection))]
[Trait("Service Integration Tests", "Integration tests for the application's Redis key prefix.")]
public class RedisKeyPrefixTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task EveryRedisKeyOfTheApplication_ShouldStartWithItsPrefix()
    {
        // Arrange: the login writes a rate limit counter, the authenticated request caches the user's revocation stamp.
        var prefix = _factory.Services.GetRequiredService<CacheKeyOptions>().Prefix;

        var user = await _factory.CreateAndLoginUserAsync();

        // Act
        await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        var keys = await GetRedisKeysAsync();

        // Assert: the template has no [Cache] method; its keys would be hashes that carry the prefix inside the hash.
        prefix.Should().NotBeNullOrWhiteSpace();
        keys.Should().Contain($"{prefix}:sstamp:{user.Id}");
        keys.Should().Contain(k => k.StartsWith($"{prefix}:rl:account-login:"));
        keys.Should().OnlyContain(k => k.StartsWith($"{prefix}:"));
    }

    [Fact]
    public async Task TwoApplicationsOnOneRedis_ShouldNotShareRateLimitCounters()
    {
        // Arrange: a second application on the same Redis (and database) with its own prefix, called from the same address.
        await using var otherApplication = _factory.CreateHost(new() { [$"{CacheKeyOptions.SectionName}:{nameof(CacheKeyOptions.Prefix)}"] = "other-application" });

        const string address = "10.250.0.1";

        var client = _factory.CreateClientWithAddress(address);
        var otherClient = otherApplication.CreateClientWithAddress(address);

        // Act: exhaust the login limit (10 per minute) of the first application.
        for (var i = 0; i < 10; i++)
            await client.RequestLoginAsync("missing-user", "Wrong1234!");

        var (limitedResponse, _) = await client.RequestLoginAsync("missing-user", "Wrong1234!");
        var (otherResponse, _) = await otherClient.RequestLoginAsync("missing-user", "Wrong1234!");

        // Assert
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        otherResponse.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    private async Task<List<string>> GetRedisKeysAsync()
    {
        var multiplexer = _factory.Services.GetRequiredService<IConnectionMultiplexer>();

        var server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);

        var keys = new List<string>();

        await foreach (var key in server.KeysAsync(pattern: "*"))
            keys.Add(key.ToString());

        return keys;
    }
}
