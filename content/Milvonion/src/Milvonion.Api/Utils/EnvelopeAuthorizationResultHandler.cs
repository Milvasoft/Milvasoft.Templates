using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Milvasoft.Components.Rest.Enums;
using Milvonion.Api.Middlewares;
using Milvonion.Application.Utils.Constants;

namespace Milvonion.Api.Utils;

/// <summary>
/// Makes a policy/role denial ("[Auth(permission)]" on an authenticated caller) answer with the same JSON envelope
/// as every other failure (403 + Forbidden message) instead of an empty 403 body. Challenges (401) and successes are
/// delegated to the default handler, whose JWT events already write the envelope.
/// </summary>
public sealed class EnvelopeAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    /// <inheritdoc/>
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden && !context.Response.HasStarted)
            return ExceptionMiddleware.RewriteResponseAsync(context, MessageKey.Forbidden, StatusCodes.Status403Forbidden, MessageType.Warning);

        return _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
