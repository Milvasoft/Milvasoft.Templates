﻿using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Helpers;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Interception.Ef.Transaction;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain;
using Milvonion.Domain.UI;
using Milvonion.Infrastructure.Persistence.Context;
using System.Net.Sockets;

namespace Milvonion.Infrastructure.Persistence;

/// <summary>
/// Data seed methods.
/// </summary>
/// <param name="serviceProvider"></param>
public class DatabaseMigrator(IServiceProvider serviceProvider)
{
    private readonly MilvonionDbContext _milvonionDbContext = serviceProvider.GetService<MilvonionDbContext>();

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> ResetDatabaseAsync(IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        await _milvonionDbContext.Database.EnsureDeletedAsync(cancellationToken);

        var connectionString = configuration.GetConnectionString("DefaultConnectionString");

        var opt = new DbContextOptionsBuilder<MilvonionDbContext>()
                    .UseNpgsql(connectionString, b => b.MigrationsHistoryTable(TableNames.EfMigrationHistory).MigrationsAssembly("Milvonion.Api").EnableRetryOnFailure())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

        opt.ConfigureWarnings(warnings => { warnings.Log(RelationalEventId.PendingModelChangesWarning); });

        try
        {
            using var db = new MilvonionDbContext(opt.Options);

            await db.Database.MigrateAsync(cancellationToken: cancellationToken);

        }
        catch (Exception ex)
        {
            // Retry
            if (ex.InnerException is SocketException || ex.InnerException is IOException)
            {
                using var db = new MilvonionDbContext(opt.Options);

                await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            }
            else
                throw;
        }

        return Response.Success();
    }

    /// <summary>
    /// Creates default triggers.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task CreateTriggersAsync(CancellationToken cancellationToken)
    {
        var createTriggerSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "create_triggers.sql"), cancellationToken);

        await _milvonionDbContext.Database.ExecuteSqlRawAsync(createTriggerSql, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Seeds default production data.
    /// </summary>
    /// <returns></returns>
    public async Task<string> SeedDefaultDataAsync(string rootPass = null, CancellationToken cancellationToken = default)
    {
        var superAdminPermission = new Permission
        {
            Id = 1,
            Name = nameof(PermissionCatalog.App.SuperAdmin),
            Description = "Provides access to the entire system.",
            NormalizedName = nameof(PermissionCatalog.App.SuperAdmin).MilvaNormalize(),
            PermissionGroup = nameof(PermissionCatalog.App),
            PermissionGroupDescription = "Application-wide permissions."
        };

        await _milvonionDbContext.Permissions.AddAsync(superAdminPermission, cancellationToken);

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

        await _milvonionDbContext.Roles.AddAsync(superAdminRole, cancellationToken);

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

        rootPass ??= IdentityHelpers.GenerateRandomPassword(16, true, true, true, true);

        _milvonionDbContext.ServiceProvider.GetService<IMilvaUserManager<User, int>>().SetPasswordHash(rootUser, rootPass);

        await _milvonionDbContext.Users.AddAsync(rootUser, cancellationToken);

        await _milvonionDbContext.SaveChangesAsync(cancellationToken);

        return rootPass;
    }

    /// <summary>
    /// Seeds default ui related data.
    /// </summary>
    /// <returns></returns>
    public async Task SeedUIRelatedDataAsync(CancellationToken cancellationToken = default)
    {
        var menuGroups = await _milvonionDbContext.MenuGroups.Select(i => i.Id).ToListAsync(cancellationToken: cancellationToken);

        if (menuGroups.Count != 0)
            return;

        #region UI

        var managementGroup = new MenuGroup
        {
            Id = 21,
            Order = 1,
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
            Order = 0,
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

        _milvonionDbContext.MenuGroups.Add(managementGroup);
        _milvonionDbContext.MenuGroups.Add(generalGroup);

        var menuItems = new List<MenuItem>
            {
                new() {
                     Id = 21,
                     Order = 99,
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
                             Order = 98,
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
                             Order = 97,
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
                     Order = 89,
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

        _milvonionDbContext.MenuItems.AddRange(menuItems);

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

        _milvonionDbContext.Pages.AddRange(pages);

        await _milvonionDbContext.SaveChangesAsync(cancellationToken);

        #endregion
    }

    /// <summary>
    /// Seeds fake data.
    /// </summary>
    /// <returns></returns>
    [Transaction]
    public async Task SeedFakeDataAsync(bool sameData = true, string locale = "tr", CancellationToken cancellationToken = default)
    {
        var roleFaker = new RoleFaker(sameData, locale);

        var roles = roleFaker.Generate(100);

        await _milvonionDbContext.BulkInsertAsync(roles, cancellationToken: cancellationToken);

        var userFaker = new UserFaker(sameData, locale, roles);

        var users = userFaker.Generate(250);

        await _milvonionDbContext.BulkInsertAsync(users, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Migrate default permissions.
    /// </summary>
    /// <param name="permissionManager"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<Response<string>> MigratePermissionsAsync(IPermissionManager permissionManager, CancellationToken cancellationToken = default) => permissionManager.MigratePermissionsAsync(cancellationToken);

    /// <summary>
    /// Initial data seed and migration operation for production.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public async Task<Response<string>> InitDatabaseAsync(IPermissionManager permissionManager, CancellationToken cancellationToken = default)
    {
        try
        {
            var initialMigrationSql = await File.ReadAllTextAsync(Path.Combine(GlobalConstant.SqlFilesPath, "initial_migration_fetch.sql"), cancellationToken);

            var initialMigration = await _milvonionDbContext.Database.SqlQueryRaw<EfMigrationHistory>(initialMigrationSql).FirstOrDefaultAsync(cancellationToken);

            if (initialMigration == null)
                return Response<string>.Error("Initial migration cannot found!");

            var migrationLog = await _milvonionDbContext.MigrationHistory.FirstOrDefaultAsync(m => m.MigrationId == initialMigration.MigrationId, cancellationToken: cancellationToken);

            if (migrationLog?.MigrationCompleted ?? false)
                return Response<string>.Error("Already initialized!");

            await _milvonionDbContext.Database.EnsureCreatedAsync(cancellationToken);

            await _milvonionDbContext.Database.MigrateAsync(cancellationToken);

            await SeedUIRelatedDataAsync(cancellationToken);

            var rootPass = await SeedDefaultDataAsync(cancellationToken: cancellationToken);

            await _milvonionDbContext.SaveChangesAsync(cancellationToken);

            await CreateTriggersAsync(cancellationToken);

            var languages = LanguagesSeed.Seed.Select(l => new Language
            {
                Code = l.Code,
                Name = l.Name,
                IsDefault = l.IsDefault,
                Supported = l.Supported,
            });

            await _milvonionDbContext.Languages.AddRangeAsync(languages, cancellationToken);

            await _milvonionDbContext.SaveChangesAsync(cancellationToken);

            var languageSeed = languages.Cast<ILanguage>().ToList();

            MultiLanguageManager.UpdateLanguagesList(languageSeed);

            await MigratePermissionsAsync(permissionManager, cancellationToken);

            if (migrationLog is null)
            {
                migrationLog = new MigrationHistory
                {
                    MigrationId = initialMigration.MigrationId,
                    MigrationCompleted = true
                };

                await _milvonionDbContext.MigrationHistory.AddAsync(migrationLog, cancellationToken);
                await _milvonionDbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await _milvonionDbContext.MigrationHistory.Where(m => m.MigrationId == migrationLog.MigrationId)
                                                          .ExecuteUpdateAsync(i => i.SetProperty(x => x.MigrationCompleted, true), cancellationToken: cancellationToken);
            }

            return Response<string>.Success(rootPass);
        }
        catch (Exception)
        {
            return Response<string>.Error("Already initialized!");
        }
    }
}
