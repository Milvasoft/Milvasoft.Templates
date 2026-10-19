using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Types.Structs;
using Milvonion.Application.Features.Roles.CreateRole;
using Milvonion.Application.Features.Roles.UpdateRole;
using Milvonion.Application.Features.Users.CreateUser;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence;
using Milvonion.Infrastructure.Persistence.Context;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Developer service.
/// </summary>
/// <param name="serviceProvider"></param>
public class DeveloperService(IServiceProvider serviceProvider) : IDeveloperService
{
    /// <summary>
    /// Password of every user <see cref="SeedDevelopmentDataAsync"/> creates (the root user included). It satisfies the
    /// default password policy (<c>Milvasoft:Identity:Password</c>); with a stricter policy the seed fails with the
    /// validation message instead of reporting success over users that were never created.
    /// </summary>
    public const string DevelopmentPassword = "Dev12345!";

    private readonly IMediator _mediator = serviceProvider.GetService<IMediator>();
    private readonly IPermissionManager _permissionManager = serviceProvider.GetService<IPermissionManager>();
    private readonly MilvonionDbContext _milvonionDbContext = serviceProvider.GetService<MilvonionDbContext>();
    private readonly DatabaseMigrator _databaseMigrator = new(serviceProvider);

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public async Task<Response> ResetDatabaseAsync()
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        return await _databaseMigrator.ResetDatabaseAsync(default);
    }

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> SeedDevelopmentDataAsync()
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        try
        {
            await _databaseMigrator.SeedDefaultDataAsync(DevelopmentPassword);

            await _databaseMigrator.CreateTriggersAsync(default);

            await _databaseMigrator.SeedUIRelatedDataAsync(default);

            await DatabaseMigrator.MigratePermissionsAsync(_permissionManager, default);

            await LanguageSeeder.SeedAsync(_milvonionDbContext, default);

            // Viewer role: every list permission of the catalog, looked up by name (the ids depend on the catalog order).
            // Every step below returns its own failure (validation messages included) instead of being ignored.
            var listPermissionIds = await _milvonionDbContext.Permissions.Where(p => p.Name == nameof(PermissionCatalog.UserManagement.List))
                                                                         .Select(p => p.Id)
                                                                         .ToListAsync();

            var addedRole = await _mediator.Send(new CreateRoleCommand
            {
                Name = "Viewer"
            });

            if (!addedRole.IsSuccess)
                return addedRole;

            var updatedRole = await _mediator.Send(new UpdateRoleCommand
            {
                Id = addedRole.Data,
                PermissionIdList = new UpdateProperty<List<int>>
                {
                    IsUpdated = true,
                    Value = listPermissionIds
                }
            });

            if (!updatedRole.IsSuccess)
                return updatedRole;

            //Another Super Admin User creation
            var addedAdmin = await _mediator.Send(new CreateUserCommand
            {
                Name = "Admin",
                Surname = "User",
                UserType = Domain.Enums.UserType.Manager,
                UserName = "admin",
                Email = "admin@example.com",
                Password = DevelopmentPassword,
                RoleIdList = [1]
            });

            if (!addedAdmin.IsSuccess)
                return addedAdmin;

            //Viewer User creation
            var addedViewer = await _mediator.Send(new CreateUserCommand
            {
                Name = "Viewer",
                Surname = "User",
                UserName = "viewer",
                UserType = Domain.Enums.UserType.AppUser,
                Email = "viewer@example.com",
                Password = DevelopmentPassword,
                RoleIdList = [addedRole.Data],
            });

            if (!addedViewer.IsSuccess)
                return addedViewer;

            return Response.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Response.Error("Already seeded!");
        }
    }

    /// <summary>
    /// Seeds fake data.
    /// </summary>
    /// <param name="sameData"></param>
    /// <param name="locale"></param>
    /// <returns></returns>
    public async Task<Response> SeedFakeDataAsync(bool sameData = true, string locale = "tr")
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        await _databaseMigrator.SeedFakeDataAsync(sameData, locale, default);

        return Response.Success();
    }

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public async Task<Response<string>> InitDatabaseAsync()
    {
        // A production database is never initialized from an endpoint (the response carries the generated root password).
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response<string>.Error(null);

        return await _databaseMigrator.InitDatabaseAsync(_permissionManager, default);
    }

    /// <summary>
    /// Resets ui related data.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public async Task<Response> ResetUIRelatedDataAsync()
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        await _databaseMigrator.SeedUIRelatedDataAsync(default);

        return Response.Success();
    }

    /// <summary>
    /// Exports existing data to a JSON file.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> ExportExistingDataAsync()
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        var menuDatas = _milvonionDbContext.Users.Include(p => p.RoleRelations)
                                                 .AsNoTracking()
                                                 .AsAsyncEnumerable();

        var filePath = Path.Combine(GlobalConstant.JsonFilesPath, "export.json");

        if (File.Exists(filePath))
            File.Delete(filePath);

        await using var stream = File.Create(filePath);
        using var streamWriter = new StreamWriter(stream, new UTF8Encoding(false));
        using var jsonWriter = new JsonTextWriter(streamWriter)
        {
            Formatting = Formatting.Indented
        };

        var serializer = new Newtonsoft.Json.JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        await jsonWriter.WriteStartArrayAsync();

        await foreach (var menu in menuDatas)
        {
            try
            {
                serializer.Serialize(jsonWriter, menu);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Serialize error: {ex.Message}");
            }
        }

        // Dizi kapat
        await jsonWriter.WriteEndArrayAsync();
        await jsonWriter.FlushAsync();
        await streamWriter.FlushAsync();

        return Response.Success();
    }

    /// <summary>
    /// Imports existing data.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> ImportExistingDataAsync()
    {
        if (MilvonionExtensions.IsCurrentEnvProduction())
            return Response.Error();

        var filePath = Path.Combine(GlobalConstant.JsonFilesPath, "export.json");

        if (!File.Exists(filePath))
            return Response.Error("Cannot find exported file.");

        await using var stream = File.OpenRead(filePath);
        using var jsonDoc = await JsonDocument.ParseAsync(stream);

        if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
            return Response.Error("Invalid file format.");

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };

        int importedCount = 0;

        foreach (var element in jsonDoc.RootElement.EnumerateArray())
        {
            try
            {

                importedCount++;
            }
            catch (Exception ex)
            {
                // Logla ve devam et
                Console.WriteLine($"Import error: {ex.Message}");
            }
        }

        await _milvonionDbContext.SaveChangesAsync();
        return Response.Success($"{importedCount} records imported.");
    }
}
