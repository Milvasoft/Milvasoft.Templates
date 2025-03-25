using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Abstract;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.IntegrationTests.TestBase;

public abstract class IntegrationTestBase(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory _factory = factory;
    protected IServiceProvider _serviceProvider;
    protected IServiceScope _serviceScope;

    public virtual Task InitializeAsync() => InitializeAsync(null, null);

    public virtual Task InitializeAsync(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configureApp = null)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnectionString", _factory.GetConnectionString());

        var waf = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                configureServices?.Invoke(services);
            });

            builder.Configure(app =>
            {
                configureApp?.Invoke(app);
            });
        });

        _serviceProvider = waf.Services.CreateScope().ServiceProvider;

        return _factory.CreateRespawner();
    }

    public virtual Task DisposeAsync() => _factory.ResetDatabase();

    public virtual async Task<User> SeedRootUserAndSuperAdminRoleAsync(string rootPassword = "defaultpass")
    {
        var dbContext = _serviceProvider.GetService<MilvonionDbContext>();

        var superAdminPermission = new Permission
        {
            Id = 1,
            Name = nameof(PermissionCatalog.App.SuperAdmin),
            Description = "Provides access to the entire system.",
            NormalizedName = nameof(PermissionCatalog.App.SuperAdmin).MilvaNormalize(),
            PermissionGroup = nameof(PermissionCatalog.App),
            PermissionGroupDescription = "Application-wide permissions."
        };

        await dbContext.Permissions.AddAsync(superAdminPermission);

        var superAdminRole = new Role
        {
            Id = 1,
            Name = nameof(PermissionCatalog.App.SuperAdmin),
            CreationDate = DateTime.Now,
            CreatorUserName = "System",
            RolePermissionRelations =
            [
                new()
                {
                    PermissionId = superAdminPermission.Id,
                    RoleId = 1
                }
            ]
        };

        await dbContext.Roles.AddAsync(superAdminRole);

        var rootUser = new User
        {
            Id = 1,
            UserName = GlobalConstant.RootUsername,
            NormalizedUserName = "ROOTUSER",
            Email = "rootuser@gmail.com",
            NormalizedEmail = "ROOTUSER@GMAIL.COM",
            Name = "Administrator",
            Surname = "User",
            UserType = Domain.Enums.UserType.Manager,
            CreationDate = DateTime.Now,
            CreatorUserName = "System",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            RoleRelations =
            [
                new()
                {
                    RoleId = superAdminRole.Id
                }
            ]
        };

        dbContext.ServiceProvider.GetService<IMilvaUserManager<User, int>>().SetPasswordHash(rootUser, rootPassword);

        await dbContext.Users.AddAsync(rootUser);

        await dbContext.SaveChangesAsync();

        return rootUser;
    }
}