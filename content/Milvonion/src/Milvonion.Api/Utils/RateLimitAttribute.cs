using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Caching.Redis.Accessor;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Helpers;

namespace Milvonion.Api.Utils;

/// <summary>
/// Per-caller rate limit for a single endpoint: a light defense-in-depth layer behind the proxy's own
/// throttling. Counts requests in a fixed time window in Redis (atomic INCR + expiry, key <see cref="CacheKeys.RateLimit"/>), keyed by the
/// authenticated user (falling back to the client IP for anonymous callers; always the client IP on
/// [AllowAnonymous] endpoints), and returns HTTP 429 once the limit is exceeded. Limits are meant to sit well above
/// normal human usage so they only bite abuse and scripts.
/// Fails OPEN: if Redis is unreachable or errors, the request is allowed through (availability over strictness).
/// </summary>
/// <param name="limit">Max requests allowed within the window.</param>
/// <param name="windowSeconds">Length of the fixed window, in seconds.</param>
/// <param name="name">Stable bucket name (defaults to the action). Endpoints sharing a name share one counter.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RateLimitAttribute(int limit, int windowSeconds, string name = null) : ActionFilterAttribute
{
    private readonly int _limit = limit;
    private readonly int _windowSeconds = windowSeconds;
    private readonly string _name = name;

    /// <inheritdoc/>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var cache = http.RequestServices.GetService<IRedisAccessor>();

        // Fail open when the cache layer is unavailable: never block a legitimate request on infrastructure.
        if (cache == null || !cache.IsConnected())
        {
            await next();
            return;
        }

        var window = _windowSeconds <= 0 ? 60 : _windowSeconds;
        var bucket = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / window;
        var key = http.RequestServices.GetRequiredService<CacheKeys>().RateLimit(_name ?? context.ActionDescriptor.DisplayName, ResolveScope(context), bucket);

        long count;

        try
        {
            var database = cache.GetDatabase();

            count = await database.StringIncrementAsync(key);

            // Set the window's expiry on the first hit so the counter self-resets.
            if (count == 1)
                await database.KeyExpireAsync(key, TimeSpan.FromSeconds(window));
        }
        catch
        {
            // Any Redis error (timeout, disconnect, MISCONF write block): allow the request through.
            await next();
            return;
        }

        if (count > _limit)
        {
            http.Response.Headers.RetryAfter = (window - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % window)).ToString();

            var localizer = http.RequestServices.GetService<IMilvaLocalizer>();

            var response = Response.Error(localizer != null ? localizer[MessageKey.TooManyRequests] : MessageKey.TooManyRequests, MessageType.Warning);
            response.StatusCode = StatusCodes.Status429TooManyRequests;

            context.Result = new ObjectResult(response) { StatusCode = StatusCodes.Status429TooManyRequests };

            return;
        }

        await next();
    }

    // Rate-limit scope: the authenticated user when present, else the client IP. The IP is taken from the
    // connection only, never from a raw X-Real-IP / X-Forwarded-For header a client could spoof to escape
    // its bucket. The forwarded-headers middleware (Program.cs) rewrites RemoteIpAddress from the trusted
    // proxy's header before this runs. Anonymous flows ([AllowAnonymous]: login, refresh) always count per IP:
    // a token attached there is not what the endpoint authenticates, so it must not open a separate bucket for
    // every account the caller holds.
    private static string ResolveScope(ActionExecutingContext context)
    {
        var http = context.HttpContext;
        var isAnonymousEndpoint = context.ActionDescriptor.EndpointMetadata?.OfType<IAllowAnonymous>().Any() == true;

        var userId = isAnonymousEndpoint ? null : http.User?.FindFirst(GlobalConstant.UserIdClaimName)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
            return $"u:{userId}";

        var ip = http.Connection.RemoteIpAddress?.ToString() ?? GlobalConstant.DefaultIp;

        return $"ip:{ip}";
    }
}
