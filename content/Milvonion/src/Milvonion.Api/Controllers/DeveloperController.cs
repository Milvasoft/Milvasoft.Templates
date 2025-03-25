using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Developer endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
public class DeveloperController(IDeveloperService developerService) : ControllerBase
{
    private readonly IDeveloperService _developerService = developerService;

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/reset")]
    public Task<Response> MigrateAsync() => _developerService.ResetDatabaseAsync();

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed")]
    public Task<Response> SeedDataAsync() => _developerService.SeedDevelopmentDataAsync();

    /// <summary>
    /// Seeds fake data for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed/fake")]
    public Task<Response> SeedFakeDataAsync(bool sameData = true, string locale = "tr") => _developerService.SeedFakeDataAsync(sameData, locale);

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/init")]
    public Task<Response<string>> InitDatabaseAsync() => _developerService.InitDatabaseAsync();

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("apilogs")]
    public Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest) => _developerService.GetApiLogsAsync(listRequest);

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("methodlogs")]
    public Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest) => _developerService.GetMethodLogsAsync(listRequest);
}