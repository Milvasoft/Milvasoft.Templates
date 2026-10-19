using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Api.Utils;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Attributes;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Developer endpoints. Every endpoint requires a super admin and is disabled in production.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion(GlobalConstant.CurrentApiVersion)]
[ApiExplorerSettings(GroupName = "v1.0")]
[Auth(PermissionCatalog.App.SuperAdmin)]
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
    /// Seeds data for development purposes. Accepts the documentation token before the root user exists (no stamp check);
    /// it seeds only an empty database ("Already seeded!" otherwise).
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed")]
    [SkipSessionStampValidation]
    public Task<Response> SeedDataAsync() => _developerService.SeedDevelopmentDataAsync();

    /// <summary>
    /// Seeds fake data for development purposes.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/seed/fake")]
    public Task<Response> SeedFakeDataAsync(bool sameData = true, string locale = "tr") => _developerService.SeedFakeDataAsync(sameData, locale);

    /// <summary>
    /// Initial migration operation. Accepts the documentation token before the root user exists (no stamp check);
    /// it initializes only an uninitialized database ("Already initialized!" otherwise).
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/init")]
    [SkipSessionStampValidation]
    public async Task<Response> InitDatabaseAsync() => await _developerService.InitDatabaseAsync();

    /// <summary>
    /// Resets ui related data.
    /// </summary>
    /// <returns></returns>
    [HttpPost("database/reset/ui")]
    public async Task<Response> ResetUIRelatedDataAsync() => await _developerService.ResetUIRelatedDataAsync();

    /// <summary>
    /// Exports product related data.
    /// </summary>
    /// <returns></returns>
    [HttpGet("export/productRelatedData")]
    public Task<Response> ExportExistingDataAsync() => _developerService.ExportExistingDataAsync();

    /// <summary>
    /// Imports exported product related data. 
    /// </summary>
    /// <returns></returns>
    [HttpGet("import/productRelatedData")]
    public Task<Response> ImportExistingDataAsync() => _developerService.ImportExistingDataAsync();
}