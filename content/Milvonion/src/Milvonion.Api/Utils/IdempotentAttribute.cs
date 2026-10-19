using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Milvonion.Api.Utils;

/// <summary>
/// Makes a create endpoint safe to retry. The client sends the same <c>Idempotency-Key</c> header for every
/// attempt at one user action (a double tap, a request the network lost, the single retry the HTTP layer does
/// after a token refresh); the first attempt claims the key and does the work, later attempts get that
/// attempt's response back instead of creating a second record.
/// </summary>
/// <remarks>
/// <para> Only endpoints that create a row with no natural uniqueness need this (e.g. a role: two roles may share a
///        name). Upserts, conditional writes and creates guarded by a unique index are idempotent on their own. </para>
/// <para> Requests without the header run untouched, so an older client keeps working. </para>
/// <para> This is a resource filter, not an action filter, so the raw body is still readable: the key is pinned
///        to a hash of the route and the body it was first used with, and the same key with a different body is
///        rejected instead of replaying the wrong response. </para>
/// <para> Cost: one extra INSERT on the first attempt; a retry pays one read and answers without doing the work.
///        Keys are stored per caller, so one user's key can never replay another's response. </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IdempotentAttribute : Attribute, IAsyncResourceFilter
{
    /// <summary>Header carrying the client-generated key.</summary>
    public const string HeaderName = "Idempotency-Key";

    /// <summary>Longest accepted key (a UUID is 36 characters).</summary>
    public const int MaxKeyLength = 64;

    /// <summary>Bytes of the request body that go into the hash; the guarded endpoints send far less.</summary>
    public const int MaxHashedBodyBytes = 256 * 1024;

    /// <inheritdoc/>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;

        if (!http.Request.Headers.TryGetValue(HeaderName, out var header))
        {
            await next();
            return;
        }

        var key = header.ToString()?.Trim();

        if (string.IsNullOrEmpty(key) || !IsWellFormed(key))
        {
            context.Result = Failure(http, MessageKey.InvalidIdempotencyKey, StatusCodes.Status400BadRequest);
            return;
        }

        var userId = ResolveUserId(http);
        var store = http.RequestServices.GetService<IIdempotencyStore>();

        // Anonymous callers have no key scope, so there is nothing to dedupe against.
        if (userId == null || store == null)
        {
            await next();
            return;
        }

        var endpoint = Truncate($"{http.Request.Method} {context.ActionDescriptor.AttributeRouteInfo?.Template ?? context.ActionDescriptor.DisplayName}");
        var requestHash = await ComputeRequestHashAsync(http, endpoint);

        var claim = await store.ClaimAsync(userId.Value, key, endpoint, requestHash, http.RequestAborted);

        if (claim.RequestMismatch)
        {
            context.Result = Failure(http, MessageKey.IdempotencyKeyReused, StatusCodes.Status409Conflict);
            return;
        }

        // The first attempt already answered: replay its response byte for byte, without doing the work again.
        if (claim.ResponseData != null)
        {
            context.Result = new ContentResult
            {
                Content = claim.ResponseData,
                ContentType = "application/json; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };

            return;
        }

        // The first attempt is still running; the client retries in a moment and gets its response.
        if (!claim.Claimed)
        {
            http.Response.Headers.RetryAfter = "1";

            context.Result = Failure(http, MessageKey.RequestInProgress, StatusCodes.Status409Conflict);

            return;
        }

        var executed = await next();

        // A failed attempt releases the key so the user can act again right away. CancellationToken.None: the
        // release must still happen when the caller disconnected mid-request.
        if (executed.Exception != null && !executed.ExceptionHandled)
        {
            await store.ReleaseAsync(userId.Value, key, CancellationToken.None);
            return;
        }

        if (executed.Result is ObjectResult { Value: IResponse envelope } objectResult && envelope.IsSuccess)
        {
            var serializerOptions = http.RequestServices.GetService<IOptions<JsonOptions>>()?.Value.JsonSerializerOptions;

            await store.CompleteAsync(userId.Value, key, JsonSerializer.Serialize(objectResult.Value, serializerOptions), CancellationToken.None);

            return;
        }

        await store.ReleaseAsync(userId.Value, key, CancellationToken.None);
    }

    // Keys are opaque to the server; only the shape is checked so a stray header cannot become a huge column value.
    private static bool IsWellFormed(string key)
    {
        if (key.Length > MaxKeyLength)
            return false;

        foreach (var character in key)
            if (!char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_')
                return false;

        return true;
    }

    private static int? ResolveUserId(HttpContext http)
        => int.TryParse(http.User?.FindFirst(GlobalConstant.UserIdClaimName)?.Value, out var userId) ? userId : null;

    // The hash pins the key to its request: the same key with a different body is a client bug, not a retry.
    // Buffering is enabled here, before model binding, so the body can be hashed and then bound as usual.
    private static async Task<byte[]> ComputeRequestHashAsync(HttpContext http, string endpoint)
    {
        var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        hash.AppendData(Encoding.UTF8.GetBytes(endpoint));

        if (http.Request.ContentLength is > 0 || http.Request.Headers.TransferEncoding.Count > 0)
        {
            http.Request.EnableBuffering();

            var buffer = new byte[8 * 1024];
            var hashed = 0;
            int read;

            while (hashed < MaxHashedBodyBytes && (read = await http.Request.Body.ReadAsync(buffer, http.RequestAborted)) > 0)
            {
                hash.AppendData(buffer, 0, read);
                hashed += read;
            }

            http.Request.Body.Position = 0;
        }

        return hash.GetHashAndReset();
    }

    private static string Truncate(string endpoint) => endpoint.Length <= 120 ? endpoint : endpoint[..120];

    private static ObjectResult Failure(HttpContext http, string messageKey, int statusCode)
    {
        var localizer = http.RequestServices.GetService<IMilvaLocalizer>();

        var response = Response.Error(localizer != null ? localizer[messageKey] : messageKey, MessageType.Warning);
        response.StatusCode = statusCode;

        return new ObjectResult(response) { StatusCode = statusCode };
    }
}
