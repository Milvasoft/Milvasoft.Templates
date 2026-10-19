using Microsoft.Extensions.Diagnostics.HealthChecks;
using Milvasoft.Caching.Redis.Accessor;
using Milvonion.Application.Interfaces;

namespace Milvonion.Api.Utils;

/// <summary>
/// Readiness checks behind <c>GET HealthCheck/ready</c>: the service is ready only when its database and Redis
/// answer. The plain <c>GET HealthCheck</c> stays a liveness probe that touches nothing.
/// </summary>
public static class ReadinessChecks
{
    /// <summary>Tag of the checks the readiness endpoint runs.</summary>
    public const string ReadyTag = "ready";

    /// <summary>
    /// Registers the database and Redis readiness checks.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddReadinessChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag])
                .AddCheck<RedisHealthCheck>("redis", tags: [ReadyTag]);

        return services;
    }
}

/// <summary>The application database accepts connections.</summary>
/// <param name="dbContextAccessor"></param>
public sealed class DatabaseHealthCheck(IMilvonionDbContextAccessor dbContextAccessor) : IHealthCheck
{
    private readonly IMilvonionDbContextAccessor _dbContextAccessor = dbContextAccessor;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContextAccessor.GetDbContext().Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("The database does not accept connections.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("The database check failed.", ex);
        }
    }
}

/// <summary>Redis answers a PING.</summary>
/// <param name="redisAccessor"></param>
public sealed class RedisHealthCheck(IRedisAccessor redisAccessor) : IHealthCheck
{
    private readonly IRedisAccessor _redisAccessor = redisAccessor;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_redisAccessor.IsConnected())
                return HealthCheckResult.Unhealthy("Redis is not connected.");

            var latency = await _redisAccessor.GetDatabase().PingAsync();

            return HealthCheckResult.Healthy($"PING {latency.TotalMilliseconds:0} ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("The Redis check failed.", ex);
        }
    }
}
