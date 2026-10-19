using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Helpers;
using Milvonion.Application.Utils.Models;
using Milvonion.UnitTests.TestSupport;
using Moq;
using StackExchange.Redis;
using System.Net;
using System.Security.Claims;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="RateLimitAttribute"/>: fixed-window counter in Redis, fail-open, user-or-ip scoped.
/// </summary>
[Trait("Api Unit Tests", "Rate limit action filter.")]
public class RateLimitAttributeTests
{
    private sealed class Harness
    {
        public Mock<IRedisAccessor> Redis { get; } = new();

        public Mock<IDatabase> Database { get; } = new();

        public List<string> IncrementedKeys { get; } = [];

        public List<(string Key, TimeSpan? Expiry)> Expirations { get; } = [];

        public int NextCalls { get; private set; }

        public ActionExecutingContext Context { get; }

        public Harness(long countAfterIncrement = 1, bool connected = true, int? userId = null, string ip = "10.0.0.7", bool throwOnIncrement = false, bool allowAnonymous = false, string keyPrefix = "app")
        {
            Redis.Setup(r => r.IsConnected()).Returns(connected);
            Redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(Database.Object);

            var increment = Database.Setup(d => d.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()))
                                    .Callback((RedisKey key, long _, CommandFlags _) => IncrementedKeys.Add(key.ToString()));

            if (throwOnIncrement)
                increment.ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, CommandFlags.None, "down", null, CommandStatus.Unknown));
            else
                increment.ReturnsAsync(countAfterIncrement);

            // The attribute calls KeyExpireAsync(key, TimeSpan); cover both overloads the compiler may bind to.
            Database.Setup(d => d.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<CommandFlags>()))
                    .Callback((RedisKey key, TimeSpan? expiry, CommandFlags _) => Expirations.Add((key.ToString(), expiry)))
                    .ReturnsAsync(true);

            Database.Setup(d => d.KeyExpireAsync(It.IsAny<RedisKey>(), It.IsAny<TimeSpan?>(), It.IsAny<ExpireWhen>(), It.IsAny<CommandFlags>()))
                    .Callback((RedisKey key, TimeSpan? expiry, ExpireWhen _, CommandFlags _) => Expirations.Add((key.ToString(), expiry)))
                    .ReturnsAsync(true);

            var services = new ServiceCollection();
            services.AddSingleton(Redis.Object);
            services.AddSingleton(FakeLocalizer.Create().Object);
            services.AddSingleton(new CacheKeys(new CacheKeyOptions { Prefix = keyPrefix }));

            var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
            http.Connection.RemoteIpAddress = ip == null ? null : IPAddress.Parse(ip);

            if (userId.HasValue)
                http.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(GlobalConstant.UserIdClaimName, userId.Value.ToString())], "test"));

            var actionDescriptor = new ActionDescriptor { DisplayName = "Milvonion.Api.Controllers.AccountController.Login" };

            if (allowAnonymous)
                actionDescriptor.EndpointMetadata = [new AllowAnonymousAttribute()];

            var actionContext = new ActionContext(http, new RouteData(), actionDescriptor);

            Context = new ActionExecutingContext(actionContext, [], new Dictionary<string, object>(), controller: new object());
        }

        public Task<ActionExecutedContext> Next()
        {
            NextCalls++;

            return Task.FromResult<ActionExecutedContext>(null);
        }
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldCallNext_WhenUnderLimit()
    {
        // Arrange
        var harness = new Harness(countAfterIncrement: 3);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.NextCalls.Should().Be(1);
        harness.Context.Result.Should().BeNull();
        harness.Context.HttpContext.Response.Headers.RetryAfter.ToString().Should().BeEmpty();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldAllowExactlyTheLimit()
    {
        // Arrange: the Nth request (count == limit) is still allowed; only N+1 is refused.
        var harness = new Harness(countAfterIncrement: 5);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.NextCalls.Should().Be(1);
        harness.Context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldReturn429WithRetryAfter_WhenOverLimit()
    {
        // Arrange
        var harness = new Harness(countAfterIncrement: 6);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.NextCalls.Should().Be(0);

        var result = harness.Context.Result.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);

        var envelope = result.Value.Should().BeAssignableTo<IResponse>().Subject;
        envelope.IsSuccess.Should().BeFalse();
        envelope.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        envelope.Messages.Should().ContainSingle().Which.Message.Should().Be(MessageKey.TooManyRequests);

        var retryAfter = int.Parse(harness.Context.HttpContext.Response.Headers.RetryAfter.ToString());
        retryAfter.Should().BeInRange(1, 60);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldSetExpiryOnlyOnFirstHitOfWindow()
    {
        // Arrange
        var first = new Harness(countAfterIncrement: 1);
        var second = new Harness(countAfterIncrement: 2);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 30);

        // Act
        await attribute.OnActionExecutionAsync(first.Context, first.Next);
        await attribute.OnActionExecutionAsync(second.Context, second.Next);

        // Assert
        first.Expirations.Should().ContainSingle().Which.Expiry.Should().Be(TimeSpan.FromSeconds(30));
        second.Expirations.Should().BeEmpty();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldFailOpen_WhenRedisNotConnected()
    {
        // Arrange
        var harness = new Harness(countAfterIncrement: 999, connected: false);
        var attribute = new RateLimitAttribute(limit: 1, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.NextCalls.Should().Be(1);
        harness.Context.Result.Should().BeNull();
        harness.Database.Verify(d => d.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldFailOpen_WhenRedisAccessorMissing()
    {
        // Arrange
        var http = new DefaultHttpContext { RequestServices = new ServiceCollection().BuildServiceProvider() };
        var context = new ActionExecutingContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), [], new Dictionary<string, object>(), new object());
        var nextCalls = 0;

        // Act
        await new RateLimitAttribute(1, 60).OnActionExecutionAsync(context, () => { nextCalls++; return Task.FromResult<ActionExecutedContext>(null); });

        // Assert
        nextCalls.Should().Be(1);
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldFailOpen_WhenRedisThrows()
    {
        // Arrange
        var harness = new Harness(throwOnIncrement: true);
        var attribute = new RateLimitAttribute(limit: 1, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.NextCalls.Should().Be(1);
        harness.Context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldScopeKeyByUser_WhenAuthenticated()
    {
        // Arrange
        var harness = new Harness(userId: 42);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60, name: "login");

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        var key = harness.IncrementedKeys.Should().ContainSingle().Subject;
        key.Should().StartWith("app:rl:login:u:42:");
        key.Should().NotContain("ip:");
        long.Parse(key.Split(':')[^1]).Should().Be(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldScopeKeyByIp_WhenAnonymous()
    {
        // Arrange
        var harness = new Harness(ip: "203.0.113.9");
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60, name: "account-login");

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.IncrementedKeys.Should().ContainSingle().Which.Should().StartWith("app:rl:account-login:ip:203.0.113.9:");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldScopeKeyByIp_OnAnonymousEndpointEvenWithAToken()
    {
        // Arrange: an [AllowAnonymous] flow (login) called with a valid token of an account the caller owns.
        var harness = new Harness(userId: 42, ip: "203.0.113.9", allowAnonymous: true);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60, name: "account-login");

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        var key = harness.IncrementedKeys.Should().ContainSingle().Subject;
        key.Should().StartWith("app:rl:account-login:ip:203.0.113.9:");
        key.Should().NotContain("u:42");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldKeepTheCountersOfTwoApplicationsApart()
    {
        // Arrange: the same caller and bucket in two applications that share one Redis.
        var firstApplication = new Harness(ip: "203.0.113.9", keyPrefix: "first-app");
        var secondApplication = new Harness(ip: "203.0.113.9", keyPrefix: "second-app");
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60, name: "account-login");

        // Act
        await attribute.OnActionExecutionAsync(firstApplication.Context, firstApplication.Next);
        await attribute.OnActionExecutionAsync(secondApplication.Context, secondApplication.Next);

        // Assert
        var firstKey = firstApplication.IncrementedKeys.Should().ContainSingle().Subject;
        var secondKey = secondApplication.IncrementedKeys.Should().ContainSingle().Subject;

        firstKey.Should().StartWith("first-app:rl:account-login:");
        secondKey.Should().StartWith("second-app:rl:account-login:");
        firstKey.Should().NotBe(secondKey);
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldUseDefaultIp_WhenNoRemoteAddress()
    {
        // Arrange
        var harness = new Harness(ip: null);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60, name: "x");

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.IncrementedKeys.Should().ContainSingle().Which.Should().StartWith($"app:rl:x:ip:{GlobalConstant.DefaultIp}:");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldUseActionDisplayName_WhenNoNameGiven()
    {
        // Arrange
        var harness = new Harness();
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 60);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.IncrementedKeys.Should().ContainSingle().Which.Should().StartWith("app:rl:Milvonion.Api.Controllers.AccountController.Login:ip:");
    }

    [Fact]
    public async Task OnActionExecutionAsync_ShouldUse60SecondWindow_WhenWindowNotPositive()
    {
        // Arrange
        var harness = new Harness(countAfterIncrement: 1);
        var attribute = new RateLimitAttribute(limit: 5, windowSeconds: 0);

        // Act
        await attribute.OnActionExecutionAsync(harness.Context, harness.Next);

        // Assert
        harness.Expirations.Should().ContainSingle().Which.Expiry.Should().Be(TimeSpan.FromSeconds(60));
    }
}
