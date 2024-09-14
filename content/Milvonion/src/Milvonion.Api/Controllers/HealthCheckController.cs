using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;
using Serilog;
using System.Net;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Health check endpoints.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1.0")]
[AllowAnonymous]
public class HealthCheckController : ControllerBase
{
    /// <summary>
    /// If api is healthy return Ok response.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<string>((int)HttpStatusCode.OK)]
    public IActionResult HealthCheck() => Ok("Ok");

    /// <summary>
    /// Alert endpoint for sending notifications.
    /// </summary>
    /// <param name="defaultNotification"></param>
    /// <returns></returns>
    [HttpPost("Alert")]
    public void Alert(DefaultNotification defaultNotification) => Log.Logger.Error(defaultNotification.Message);
}