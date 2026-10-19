using FluentAssertions;
using Milvonion.Api.Controllers;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Utils.Constants;
using Milvonion.IntegrationTests.TestBase;
using System.Net;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the liveness and readiness probes.")]
public class HealthCheckControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ReadyAsync_WhenTheDatabaseAndRedisAnswer_ShouldBe200()
    {
        // Act
        var httpResponse = await _factory.CreateClientWithAddress().GetAsync($"{TestApi.Base}/healthcheck/ready", TestContext.Current.CancellationToken);
        var report = await TestApi.ReadAsync<ReadinessReport>(httpResponse);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        report.Status.Should().Be("Healthy");
        report.Checks.Should().ContainKeys("database", "redis");
        report.Checks.Values.Should().OnlyContain(c => c.Status == "Healthy");
    }

    [Fact]
    public async Task ReadyAsync_WhenRedisIsDown_ShouldBe503WhileTheRestKeepsWorking()
    {
        // Arrange: a host whose Redis endpoint refuses connections (the shared database, so the root user exists).
        await using var redisDownHost = _factory.CreateHost(new() { [MilvonionApiFactory.RedisConnectionStringKey] = "127.0.0.1:1" });

        var client = redisDownHost.CreateClientWithAddress();

        var loginRequest = new LoginCommand
        {
            UserName = "admin",
            Password = "admin",
            DeviceId = "device-id"
        };

        // Act
        var readyResponse = await client.GetAsync($"{TestApi.Base}/healthcheck/ready", TestContext.Current.CancellationToken);
        var report = await TestApi.ReadAsync<ReadinessReport>(readyResponse);
        var liveResponse = await client.GetAsync($"{TestApi.Base}/healthcheck", TestContext.Current.CancellationToken);

        // The login rate limit (10 per minute) fails open without Redis.
        var loginStatusCodes = new List<HttpStatusCode>();

        for (var i = 0; i < 12; i++)
        {
            var (loginResponse, _) = await client.PostAsync<LoginResponseDto>("account/login", loginRequest);

            loginStatusCodes.Add(loginResponse.StatusCode);
        }

        // Token revocation falls back to the database.
        var root = await redisDownHost.LoginAsync(GlobalConstant.RootUsername, TestApi.RootPassword);
        var detailResponse = await root.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        // Assert
        readyResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        report.Status.Should().Be("Unhealthy");
        report.Checks["database"].Status.Should().Be("Healthy");
        report.Checks["redis"].Status.Should().Be("Unhealthy");
        liveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginStatusCodes.Should().OnlyContain(s => s == HttpStatusCode.Unauthorized);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
