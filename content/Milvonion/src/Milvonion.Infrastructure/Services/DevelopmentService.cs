using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Interception.Ef.Transaction;
using Milvasoft.Types.Structs;
using Milvonion.Application.Features.Roles.CreateRole;
using Milvonion.Application.Features.Roles.UpdateRole;
using Milvonion.Application.Features.Users.CreateUser;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain;
using Milvonion.Domain.UI;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Development service for development purposes.
/// </summary>
/// <param name="mediator"></param>
/// <param name="permissionManager"></param>
/// <param name="milvonionDbContext"></param>
/// <param name="userManager"></param>
/// <param name="methodLogRepository"></param>
/// <param name="apiLogRepository"></param>
/// <param name="configuration"></param>
public class DevelopmentService(IMediator mediator,
                                IPermissionManager permissionManager,
                                MilvonionDbContext milvonionDbContext,
                                IMilvaUserManager<User, int> userManager,
                                IMilvonionRepositoryBase<MethodLog> methodLogRepository,
                                IMilvonionRepositoryBase<ApiLog> apiLogRepository,
                                IConfiguration configuration) : IDevelopmentService
{
    private readonly IPermissionManager _permissionManager = permissionManager;
    private readonly MilvonionDbContext _milvonionDbContext = milvonionDbContext;
    private readonly IMilvaUserManager<User, int> _userManager = userManager;
    private readonly IMilvonionRepositoryBase<MethodLog> _methodLogRepository = methodLogRepository;
    private readonly IMilvonionRepositoryBase<ApiLog> _apiLogRepository = apiLogRepository;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> ResetDatabaseAsync()
    {
        await _milvonionDbContext.Database.EnsureDeletedAsync();

        var connectionString = _configuration.GetConnectionString("DefaultConnectionString");

        var opt = new DbContextOptionsBuilder<MilvonionDbContext>()
                    .UseNpgsql(connectionString, b => b.MigrationsHistoryTable("_MigrationHistory").MigrationsAssembly("Milvonion.Api").EnableRetryOnFailure())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

        using var db = new MilvonionDbContext(opt.Options);

        await db.Database.MigrateAsync();

        var sqlText = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "dev_create_tables_and_seed.sql"));

        await db.Database.ExecuteSqlRawAsync(sqlText);

        return Response.Success();
    }

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> SeedDataAsync()
    {
        try
        {
            await SeedUIRelatedDataAsync();

            await _permissionManager.MigratePermissionsAsync();

            //Role creation
            var addedRole = await mediator.Send(new CreateRoleCommand
            {
                Name = "Viewer"
            });

            //Role creation
            await mediator.Send(new UpdateRoleCommand
            {
                Id = addedRole.Data,
                PermissionIdList = new UpdateProperty<List<int>>
                {
                    IsUpdated = true,
                    Value = [22, 27, 32, 33]
                }
            });

            //Another Super Admin User creation
            await mediator.Send(new CreateUserCommand
            {
                Name = "Ahmet Buğra",
                Surname = "Kösen",
                UserType = Domain.Enums.UserType.Manager,
                UserName = "bugrakosen",
                Email = "bugrakosen@gmail.com",
                Password = "string",
                RoleIdList = [1]
            });

            //Akbank mobil user creation
            await mediator.Send(new CreateUserCommand
            {
                Name = "Ak",
                Surname = "Bank",
                UserName = "akbank",
                UserType = Domain.Enums.UserType.Manager,
                Email = "akbank@gmail.com",
                Password = "string",
                RoleIdList = [addedRole.Data],
            });

            //Viewer User creation
            await mediator.Send(new CreateUserCommand
            {
                Name = "Viewer",
                Surname = "User",
                UserName = "viewer",
                UserType = Domain.Enums.UserType.AppUser,
                Email = "viewer@gmail.com",
                Password = "string",
                RoleIdList = [addedRole.Data],
            });

            return Response.Success();
        }
        catch (Exception)
        {
            return Response.Error("Already seeded!");
        }
    }

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [Transaction]
    [ExcludeFromMetadata]
    public async Task<Response<string>> InitDatabaseAsync()
    {
        try
        {
            var initialMigration = await _milvonionDbContext.MigrationHistory.FirstOrDefaultAsync(m => m.MigrationId.EndsWith("InitialCreate"));

            if (initialMigration == null)
                return Response<string>.Error("Initial migration cannot found!");

            if (initialMigration.MigrationCompleted)
                return Response<string>.Error("Already initialized!");

            var createTableSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "create_tables.sql"));

            await _milvonionDbContext.Database.ExecuteSqlRawAsync(createTableSql);

            var rootPass = await SeedDefaultDataAsync();

            await _milvonionDbContext.SaveChangesAsync();

            initialMigration.MigrationCompleted = true;

            await _milvonionDbContext.MigrationHistory.Where(m => m.MigrationId == initialMigration.MigrationId)
                                                    .ExecuteUpdateAsync(i => i.SetProperty(x => x.MigrationCompleted, true));

            var createTriggerSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "create_triggers.sql"));

            await _milvonionDbContext.Database.ExecuteSqlRawAsync(createTriggerSql);

            return Response<string>.Success(rootPass);
        }
        catch (Exception)
        {
            return Response<string>.Error("Already initialized!");
        }
    }

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    public async Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest) => await _methodLogRepository.GetAllAsync(listRequest);

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    public async Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest) => await _apiLogRepository.GetAllAsync(listRequest);

    private async Task<string> SeedDefaultDataAsync()
    {
        await SeedUIRelatedDataAsync();

        var superAdminPermission = new Permission
        {
            Id = 1,
            Name = nameof(PermissionCatalog.App.SuperAdmin),
            Description = "Provides access to the entire system.",
            NormalizedName = nameof(PermissionCatalog.App.SuperAdmin).MilvaNormalize(),
            PermissionGroup = nameof(PermissionCatalog.App),
            PermissionGroupDescription = "Application-wide permissions."
        };

        await _milvonionDbContext.Permissions.AddAsync(superAdminPermission);

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

        await _milvonionDbContext.Roles.AddAsync(superAdminRole);

        var rootUser = new User
        {
            Id = 1,
            UserName = "rootuser",
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

        var randomPasswordForRootUser = IdentityHelpers.GenerateRandomPassword(16, true, true, true, true);

        _userManager.SetPasswordHash(rootUser, randomPasswordForRootUser);

        return randomPasswordForRootUser;
    }

    private async Task SeedUIRelatedDataAsync()
    {

        #region UI

        var managementGroup = new MenuGroup
        {
            Id = 21,
            Translations =
            [
                new()
                    {
                        LanguageId = 1,
                        Name = "Yönetim",
                        EntityId = 21
                    },
                    new()
                    {
                        LanguageId = 2,
                        Name = "Management",
                        EntityId = 21
                    }
            ],
            CreationDate = DateTime.Now,
            CreatorUserName = "System"
        };

        var generalGroup = new MenuGroup
        {
            Id = 22,
            Translations =
            [
                new()
                    {
                        LanguageId = 1,
                        Name = "Genel",
                        EntityId = 22
                    },
                    new()
                    {
                        LanguageId = 2,
                        Name = "General",
                        EntityId = 22
                    }
            ],
            CreationDate = DateTime.Now,
            CreatorUserName = "System"
        };

        await _milvonionDbContext.MenuGroups.AddAsync(managementGroup);
        await _milvonionDbContext.MenuGroups.AddAsync(generalGroup);

        var menuItems = new List<MenuItem>
            {
                new() {
                     Id = 21,
                     GroupId = managementGroup.Id,
                     PermissionOrGroupNames = [nameof(PermissionCatalog.App), nameof(PermissionCatalog.UserManagement), nameof(PermissionCatalog.ActivityLogManagement)],
                     Translations =
                     [
                         new()
                         {
                             LanguageId = 1,
                             Name = "Kullanıcı Yönetimi",
                             EntityId = 21
                         },
                         new()
                         {
                             LanguageId = 2,
                             Name = "User Management",
                             EntityId = 21
                         }
                     ],
                     Childrens =
                     [
                         new()
                         {
                             Id = 22,
                             ParentId = 21,
                             GroupId = managementGroup.Id,
                             Url = "/users",
                             PageName = nameof(PermissionCatalog.UserManagement),
                             PermissionOrGroupNames = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.UserManagement.List],
                             Translations =
                             [
                                 new()
                                 {
                                     LanguageId = 1,
                                     Name = "Kullanıcılar",
                                     EntityId = 22
                                 },
                                 new()
                                 {
                                     LanguageId = 2,
                                     Name = "Users",
                                     EntityId = 22
                                 }
                             ],
                             CreationDate = DateTime.Now,
                             CreatorUserName = "System"
                         },
                         new()
                         {
                             Id = 23,
                             ParentId = 21,
                             GroupId = managementGroup.Id,
                             Url = "/activityLogs",
                             PageName = nameof(PermissionCatalog.ActivityLogManagement),
                             PermissionOrGroupNames = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.ActivityLogManagement.List],
                             Translations =
                             [
                                 new()
                                 {
                                     LanguageId = 1,
                                     Name = "Kullanıcı Aktiviteleri",
                                     EntityId = 23
                                 },
                                 new()
                                 {
                                     LanguageId = 2,
                                     Name = "User Activities",
                                     EntityId = 23
                                 }
                             ],
                             CreationDate = DateTime.Now,
                             CreatorUserName = "System"
                         }
                     ],
                     CreationDate = DateTime.Now,
                     CreatorUserName = "System"
                },
                new() {
                     Id = 25,
                     GroupId = managementGroup.Id,
                     Url = "/role",
                     PageName = nameof(PermissionCatalog.RoleManagement),
                     PermissionOrGroupNames = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.RoleManagement.List],
                     Translations =
                     [
                         new()
                         {
                             LanguageId = 1,
                             Name = "Roller",
                             EntityId = 25
                         },
                         new()
                         {
                             LanguageId = 2,
                             Name = "Roles",
                             EntityId = 25
                         }
                     ],
                     CreationDate = DateTime.Now,
                     CreatorUserName = "System"
                },
            };

        await _milvonionDbContext.MenuItems.AddRangeAsync(menuItems);

        var pages = new List<Page>
            {           
                // Users Page
                new()
                {
                    Id = 22,
                    Name = nameof(PermissionCatalog.UserManagement),
                    HasCreate = true,
                    HasEdit = true,
                    HasDetail = true,
                    HasDelete = true,
                    CreatePermissions = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.UserManagement.Create],
                    EditPermissions = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.UserManagement.Update],
                    DetailPermissions =  [PermissionCatalog.App.SuperAdmin, PermissionCatalog.UserManagement.Detail],
                    DeletePermissions= [PermissionCatalog.App.SuperAdmin, PermissionCatalog.UserManagement.Delete],
                    CreationDate = DateTime.Now,
                    CreatorUserName = "System",
                },
                
                // ActivityLogManagement Page
                new()
                {
                    Id = 23,
                    Name = nameof(PermissionCatalog.ActivityLogManagement),
                    HasCreate = false,
                    HasEdit = false,
                    HasDetail = false,
                    HasDelete = false,
                    CreationDate = DateTime.Now,
                    CreatorUserName = "System",
                },
                
           
                // RoleManagement Page
                new()
                {
                    Id = 25,
                    Name = nameof(PermissionCatalog.RoleManagement),
                    HasCreate = true,
                    HasEdit = true,
                    HasDetail = true,
                    HasDelete = true,
                    CreatePermissions = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.RoleManagement.Create],
                    EditPermissions = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.RoleManagement.Update],
                    DetailPermissions = [PermissionCatalog.App.SuperAdmin, PermissionCatalog.RoleManagement.Detail],
                    DeletePermissions= [PermissionCatalog.App.SuperAdmin, PermissionCatalog.RoleManagement.Delete],
                    CreationDate = DateTime.Now,
                    CreatorUserName = "System",
                },

            };

        await _milvonionDbContext.Pages.AddRangeAsync(pages);

        await _milvonionDbContext.SaveChangesAsync();

        #endregion
    }
}
