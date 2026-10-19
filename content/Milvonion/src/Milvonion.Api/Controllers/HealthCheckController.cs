using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;
using System.Net;

namespace Milvonion.Api.Controllers;

/// <summary>
/// Health check endpoints: a liveness probe and a readiness probe that checks the database and Redis.
/// </summary>
[ApiController]
[Route(GlobalConstant.FullRoute)]
[ApiVersion(GlobalConstant.CurrentApiVersion)]
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
    /// Readiness probe: 200 when the database and Redis answer, 503 otherwise, with the state of each check.
    /// </summary>
    /// <param name="healthCheckService"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    [HttpGet("ready")]
    [ProducesResponseType<ReadinessReport>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ReadinessReport>((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> ReadyAsync([FromServices] HealthCheckService healthCheckService, CancellationToken cancellation)
    {
        var report = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains(ReadinessChecks.ReadyTag), cancellation);

        var body = new ReadinessReport
        {
            Status = report.Status.ToString(),
            DurationMs = (long)report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.ToDictionary(e => e.Key, e => new ReadinessCheck { Status = e.Value.Status.ToString(), Description = e.Value.Description })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(body)
            : StatusCode((int)HttpStatusCode.ServiceUnavailable, body);
    }
}

/// <summary>Result of the readiness probe.</summary>
public sealed class ReadinessReport
{
    /// <summary>Healthy, Degraded or Unhealthy.</summary>
    public string Status { get; set; }

    /// <summary>Total time the checks took.</summary>
    public long DurationMs { get; set; }

    /// <summary>State of each check by name.</summary>
    public Dictionary<string, ReadinessCheck> Checks { get; set; }
}

/// <summary>State of one readiness check.</summary>
public sealed class ReadinessCheck
{
    /// <summary>Healthy, Degraded or Unhealthy.</summary>
    public string Status { get; set; }

    /// <summary>Short detail (never secrets or exception text).</summary>
    public string Description { get; set; }
}
