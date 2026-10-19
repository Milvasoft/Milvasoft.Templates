using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;
using Milvonion.Infrastructure.Persistence.Context;
using Milvonion.Infrastructure.Services;
using Milvonion.IntegrationTests.TestBase;
using System.Net;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the developer (bootstrap) endpoints.")]
public class DeveloperControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SeedDataAsync_OnAFreshDatabase_ShouldCreateTheDevelopmentUsersWithAPasswordThatSignsIn()
    {
        // Arrange: a database nobody initialized yet, seeded with the documentation token like on a developer machine.
        var connectionString = await _factory.CreateEmptyDatabaseAsync();

        await using var freshHost = _factory.CreateHost(new() { [MilvonionApiFactory.ConnectionStringKey] = connectionString });

        var accessToken = await freshHost.CreateClientWithAddress().GetDocumentationTokenAsync();

        var client = freshHost.CreateClientWithAddress().UseAccessToken(accessToken);

        // Act
        var seedResponse = await client.PostAsync($"{TestApi.Base}/developer/database/seed", null, TestContext.Current.CancellationToken);
        var seedResult = await TestApi.ReadAsync<Response>(seedResponse);

        var (adminLoginResponse, adminLogin) = await freshHost.CreateClientWithAddress().RequestLoginAsync("admin", DeveloperService.DevelopmentPassword);
        var (viewerLoginResponse, viewerLogin) = await freshHost.CreateClientWithAddress().RequestLoginAsync("viewer", DeveloperService.DevelopmentPassword);
        var (rootLoginResponse, _) = await freshHost.CreateClientWithAddress().RequestLoginAsync(GlobalConstant.RootUsername, DeveloperService.DevelopmentPassword);

        // Assert: the seed reports success only when every user was created, and each one signs in with the documented password.
        seedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        seedResult.IsSuccess.Should().BeTrue();
        adminLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK, adminLogin?.MessagesToString());
        viewerLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK, viewerLogin?.MessagesToString());
        rootLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = freshHost.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MilvonionDbContext>();

        var users = await dbContext.Users.Where(u => u.UserName == "admin" || u.UserName == "viewer")
                                         .OrderBy(u => u.UserName)
                                         .Select(u => new { u.UserName, u.Email, u.UserType })
                                         .ToListAsync(TestContext.Current.CancellationToken);

        var viewerPermissionNames = await dbContext.RolePermissionRelations.Where(r => r.Role.Name == "Viewer")
                                                                           .Select(r => r.Permission.Name)
                                                                           .Distinct()
                                                                           .ToListAsync(TestContext.Current.CancellationToken);

        users.Should().BeEquivalentTo([new { UserName = "admin", Email = "admin@example.com", UserType = UserType.Manager },
                                       new { UserName = "viewer", Email = "viewer@example.com", UserType = UserType.AppUser }]);
        viewerPermissionNames.Should().Equal(nameof(PermissionCatalog.UserManagement.List));
    }
}
