using MediatR;
using Microsoft.Extensions.Configuration;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.MultiLanguage.EntityBases.Abstract;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.Types.Structs;
using Milvonion.Application.Features.Roles.CreateRole;
using Milvonion.Application.Features.Roles.UpdateRole;
using Milvonion.Application.Features.Users.CreateUser;
using Milvonion.Application.Interfaces;
using Milvonion.Domain;
using Milvonion.Infrastructure.Persistence;
using Milvonion.Infrastructure.Persistence.Context;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Developer service.
/// </summary>
/// <param name="mediator"></param>
/// <param name="permissionManager"></param>
/// <param name="milvonionDbContext"></param>
/// <param name="methodLogRepository"></param>
/// <param name="apiLogRepository"></param>
/// <param name="configuration"></param>
public class DeveloperService(IMediator mediator,
                              IPermissionManager permissionManager,
                              MilvonionDbContext milvonionDbContext,
                              IMilvonionRepositoryBase<MethodLog> methodLogRepository,
                              IMilvonionRepositoryBase<ApiLog> apiLogRepository,
                              IConfiguration configuration) : IDeveloperService
{
    private readonly IMediator _mediator = mediator;
    private readonly IPermissionManager _permissionManager = permissionManager;
    private readonly IMilvonionRepositoryBase<MethodLog> _methodLogRepository = methodLogRepository;
    private readonly IMilvonionRepositoryBase<ApiLog> _apiLogRepository = apiLogRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly MilvonionDbContext _milvonionDbContext = milvonionDbContext;
    private readonly DatabaseMigrator _databaseMigrator = new(milvonionDbContext);

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    public Task<Response> ResetDatabaseAsync() => _databaseMigrator.ResetDatabaseAsync(_configuration, default);

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> SeedDevelopmentDataAsync()
    {
        try
        {
            await _databaseMigrator.SeedDefaultDataAsync("string");

            await _databaseMigrator.CreateTriggersAsync(default);

            await _databaseMigrator.SeedUIRelatedDataAsync(default);

            await DatabaseMigrator.MigratePermissionsAsync(_permissionManager, default);

            var languages = LanguagesSeed.Seed.Select(l => new Language
            {
                Code = l.Code,
                Name = l.Name,
                IsDefault = l.IsDefault,
                Supported = l.Supported,
            });

            await _milvonionDbContext.Languages.AddRangeAsync(languages, default);

            await _milvonionDbContext.SaveChangesAsync(default);

            var languageSeed = languages.Cast<ILanguage>().ToList();

            MultiLanguageManager.UpdateLanguagesList(languageSeed);

            //Role creation
            var addedRole = await _mediator.Send(new CreateRoleCommand
            {
                Name = "Viewer"
            });

            //Role creation
            await _mediator.Send(new UpdateRoleCommand
            {
                Id = addedRole.Data,
                PermissionIdList = new UpdateProperty<List<int>>
                {
                    IsUpdated = true,
                    Value = [21, 26, 31, 32]
                }
            });

            //Another Super Admin User creation
            await _mediator.Send(new CreateUserCommand
            {
                Name = "Ahmet Buğra",
                Surname = "Kösen",
                UserType = Domain.Enums.UserType.Manager,
                UserName = "bugrakosen",
                Email = "bugrakosen@gmail.com",
                Password = "string",
                RoleIdList = [1]
            });

            //Viewer User creation
            await _mediator.Send(new CreateUserCommand
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
        await _databaseMigrator.SeedFakeDataAsync(sameData, locale, default);

        return Response.Success();
    }

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public Task<Response<string>> InitDatabaseAsync() => _databaseMigrator.InitDatabaseAsync(_permissionManager, default);

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    public Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest) => _methodLogRepository.GetAllAsync(listRequest);

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <param name="listRequest"></param>
    /// <returns></returns>
    public Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest) => _apiLogRepository.GetAllAsync(listRequest);
}
