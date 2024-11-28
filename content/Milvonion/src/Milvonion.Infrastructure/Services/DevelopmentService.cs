using MediatR;
using Microsoft.Extensions.Configuration;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
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
                                IMilvonionRepositoryBase<MethodLog> methodLogRepository,
                                IMilvonionRepositoryBase<ApiLog> apiLogRepository,
                                IConfiguration configuration) : IDevelopmentService
{
    private readonly IMediator _mediator = mediator;
    private readonly IPermissionManager _permissionManager = permissionManager;
    private readonly IMilvonionRepositoryBase<MethodLog> _methodLogRepository = methodLogRepository;
    private readonly IMilvonionRepositoryBase<ApiLog> _apiLogRepository = apiLogRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly DatabaseMigrator _databaseMigrator = new(milvonionDbContext);

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    public async Task<Response> ResetDatabaseAsync() => await _databaseMigrator.ResetDatabaseAsync(_configuration, default);

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

            await _databaseMigrator.MigratePermissionsAsync(_permissionManager, default);

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
            return Response.Error("Already seeded!");
        }
    }

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromMetadata]
    public async Task<Response<string>> InitDatabaseAsync() => await _databaseMigrator.InitDatabaseAsync(_permissionManager, default);

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
}
