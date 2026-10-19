using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Milvonion.Application.Features.Roles.CreateRole;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Infrastructure.Services;
using Milvonion.IntegrationTests.TestBase;
using Milvonion.IntegrationTests.TestBase.Fakes;
using System.Net;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the role endpoints and the Idempotency-Key retry safety of role creation.")]
public class RolesControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task AddRoleAsync_RetriedWithTheSameIdempotencyKey_ShouldCreateOneRoleAndReplayTheFirstResponse()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();
        var request = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };
        var key = Guid.NewGuid().ToString("N");

        // Act
        var (firstResponse, firstResult) = await root.Client.PostWithKeyAsync<int>("roles/role", request, key);
        var (retryResponse, retryResult) = await root.Client.PostWithKeyAsync<int>("roles/role", request, key);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        retryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResult.Data.Should().BeGreaterThan(0);
        retryResult.Data.Should().Be(firstResult.Data, "the retry gets the first attempt's response back");

        (await RoleCountAsync(request.Name)).Should().Be(1);
    }

    [Fact]
    public async Task AddRoleAsync_WithoutIdempotencyKey_ShouldCreateARolePerRequest()
    {
        // Arrange: the header is opt-in, a request without it runs untouched.
        var root = await _factory.LoginAsRootAsync();
        var request = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };

        // Act
        var (firstResponse, _) = await root.Client.PostAsync<int>("roles/role", request);
        var (secondResponse, _) = await root.Client.PostAsync<int>("roles/role", request);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        (await RoleCountAsync(request.Name)).Should().Be(2);
    }

    [Fact]
    public async Task AddRoleAsync_SameIdempotencyKeyWithAnotherBody_ShouldBe409AndCreateNothing()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var firstRequest = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };
        var otherRequest = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };
        var key = Guid.NewGuid().ToString("N");

        // Act
        await root.Client.PostWithKeyAsync<int>("roles/role", firstRequest, key);

        var (httpResponse, result) = await root.Client.PostWithKeyAsync<int>("roles/role", otherRequest, key);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        result.Messages[0].Message.Should().Be("This Idempotency-Key was already used for another request.");

        (await RoleCountAsync(otherRequest.Name)).Should().Be(0);
    }

    [Fact]
    public async Task AddRoleAsync_WithAMalformedIdempotencyKey_ShouldBe400AndCreateNothing()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var request = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };

        // Act
        var (httpResponse, result) = await root.Client.PostWithKeyAsync<int>("roles/role", request, "not a valid key!");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        result.Messages[0].Message.Should().StartWith("Invalid Idempotency-Key header.");

        (await RoleCountAsync(request.Name)).Should().Be(0);
    }

    [Fact]
    public async Task AddRoleAsync_RetryWhileTheFirstAttemptRuns_ShouldBe409WithRetryAfterAndThenReplay()
    {
        // Arrange: a host whose idempotency store holds the first attempt between its work and its stored response.
        var gate = new GatedIdempotencyStore.Gate();

        await using var host = _factory.CreateHost(configureServices: services =>
        {
            services.RemoveAll<IIdempotencyStore>();
            services.AddScoped<IIdempotencyStore>(sp => new GatedIdempotencyStore(ActivatorUtilities.CreateInstance<IdempotencyStore>(sp), gate));
        });

        var root = await host.LoginAsync(GlobalConstant.RootUsername, TestApi.RootPassword);
        var request = new CreateRoleCommand { Name = $"role{TestApi.Unique()}" };
        var key = Guid.NewGuid().ToString("N");

        // Act
        var firstAttempt = root.Client.PostWithKeyAsync<int>("roles/role", request, key);

        await gate.Reached.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        var (inProgressResponse, inProgressResult) = await root.Client.PostWithKeyAsync<int>("roles/role", request, key);

        gate.Release();

        var (firstResponse, firstResult) = await firstAttempt;
        var (replayResponse, replayResult) = await root.Client.PostWithKeyAsync<int>("roles/role", request, key);

        // Assert
        inProgressResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        inProgressResponse.Headers.RetryAfter.Should().NotBeNull();
        inProgressResult.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        replayResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        replayResult.Data.Should().Be(firstResult.Data);

        (await RoleCountAsync(request.Name)).Should().Be(1);
    }

    private Task<int> RoleCountAsync(string name) => _factory.DbAsync(db => db.Roles.CountAsync(r => r.Name == name, TestContext.Current.CancellationToken));
}
