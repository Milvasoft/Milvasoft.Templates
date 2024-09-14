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
public class DevelopmentController(IDevelopmentService developmentService) : ControllerBase
{
    private readonly IDevelopmentService _developmentService = developmentService;

    /// <summary>
    /// Remove, recreates and seed database for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/reset")]
    public async Task<Response> MigrateAsync() => await _developmentService.ResetDatabaseAsync();

    /// <summary>
    /// Seeds data for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed")]
    public async Task<Response> SeedDataAsync() => await _developmentService.SeedDataAsync();

    /// <summary>
    /// Initial migration operation.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/init")]
    public async Task<Response<string>> InitDatabaseAsync() => await _developmentService.InitDatabaseAsync();

    /// <summary>
    /// Gets api logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("apilogs")]
    public async Task<ListResponse<ApiLog>> GetApiLogsAsync(ListRequest listRequest) => await _developmentService.GetApiLogsAsync(listRequest);

    /// <summary>
    /// Gets method logs.
    /// </summary>
    /// <returns></returns>
    [HttpPatch("methodlogs")]
    public async Task<ListResponse<MethodLog>> GetMethodLogsAsync(ListRequest listRequest) => await _developmentService.GetMethodLogsAsync(listRequest);
}