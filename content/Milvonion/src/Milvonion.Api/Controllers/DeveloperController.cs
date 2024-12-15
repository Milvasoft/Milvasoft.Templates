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
    public async Task<Response> MigrateAsync() => await _developerService.ResetDatabaseAsync();

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed")]
    public async Task<Response> SeedDataAsync() => await _developerService.SeedDevelopmentDataAsync();

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/init")]
    public async Task<Response<string>> InitDatabaseAsync() => await _developerService.InitDatabaseAsync();

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("apilogs")]
    public async Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest) => await _developerService.GetApiLogsAsync(listRequest);

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("methodlogs")]
    public async Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest) => await _developerService.GetMethodLogsAsync(listRequest);
}