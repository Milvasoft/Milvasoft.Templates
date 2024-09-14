using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;
using Milvonion.Domain.JsonModels;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Milvonion.Api.Middlewares;

/// <summary>
/// For request response logging.
/// </summary>
/// <param name="next"></param>
/// <param name="loggerFactory"></param>
public class RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory) : IInterceptable
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();
    private readonly Stopwatch _sw = new();
    private readonly RequestInfo _requestInfo = new();

    /// <summary>
    /// Logs request and response.  
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [ActivityStarter(GlobalConstant.LoggingActivityName)]
    public async Task Invoke(HttpContext context)
    {
        try
        {
            _sw.Restart();

            if (Ignore(context))
            {
                if (_next != null && !context.Response.HasStarted)
                    await _next.Invoke(context);

                return;
            }

            await ReadRequestAsync(context);
            await LogResponseAsync(context);
        }
        catch (Exception ex)
        {
            Serilog.Log.Logger.Error(ex, MessageConstant.ExceptionLogTemplate, ex.Message);
            await ExceptionMiddleware.RewriteResponseAsync(context, MessageKey.UnhandledException, (int)HttpStatusCode.BadRequest);
        }
    }

    private async Task ReadRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        await using var requestStream = _recyclableMemoryStreamManager.GetStream();

        await context.Request.Body.CopyToAsync(requestStream);

        _requestInfo.Method = context.Request.Method;

        _requestInfo.Body = await ReadStreamInChunksAsync(requestStream);

        _requestInfo.Headers = context.Request.Headers;

        _requestInfo.ContentLength = context.Request.ContentLength ?? 0;

        _requestInfo.AbsoluteUri = string.Concat(context.Request.Scheme,
                                                 "://",
                                                 context.Request.Host.ToUriComponent(),
                                                 context.Request.PathBase.ToUriComponent(),
                                                 context.Request.Path.ToUriComponent(),
                                                 context.Request.QueryString.ToUriComponent());

        _requestInfo.QueryString = context.Request.QueryString.ToUriComponent();

        context.Request.Body.Position = 0;
    }

    private async Task LogResponseAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        try
        {
            await using var emptyResponseBody = _recyclableMemoryStreamManager.GetStream();

            context.Response.Body = emptyResponseBody;

            if (_next != null && !context.Response.HasStarted)
                await _next.Invoke(context);

            if (IgnoreResponse(context))
                return;

            emptyResponseBody.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(emptyResponseBody);

            var responseBody = await streamReader.ReadToEndAsync();

            responseBody = Regex.Unescape(responseBody);

            emptyResponseBody.Seek(0, SeekOrigin.Begin);

            var realIpExists = context.Request.Headers.TryGetValue("X-Real-IP", out StringValues realIpAddress);

            string requestIp;

            if (realIpExists)
                requestIp = realIpAddress.ToString();
            else
            {
                requestIp = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();

                if (requestIp == "0.0.0.1")
                    requestIp = context.Connection.LocalIpAddress?.MapToIPv4().ToString();
            }

            if (!responseBody.Contains('{') && JsonNode.Parse(responseBody) is JsonObject jsonObject)
            {
                responseBody = FilterBase64FromJson(jsonObject); // Hatalı JSON döndürülüyor
            }

            var responseInfo = new ResponseInfo
            {
                StatusCode = context.Response.StatusCode,
                Body = responseBody,
                Headers = context.Response.Headers,
                Length = context.Response.ContentLength ?? Encoding.Unicode.GetByteCount(responseBody),
                ContentType = context.Response.ContentType ?? string.Empty,
            };

            _sw.Stop();

            _logger.LogInformation("{TransactionId}{Severity}{Timestamp}{Path}{@RequestInfoJson}{@ResponseInfoJson}{ElapsedMs}{IpAddress}{UserName}",
                                   ActivityHelper.Id,
                                   LogLevel.Information.ToString(),
                                   DateTimeOffset.UtcNow,
                                   context.Request.Path,
                                   _requestInfo,
                                   responseInfo,
                                   _sw.ElapsedMilliseconds,
                                   requestIp,
                                   User.GetCurrentUser(context.RequestServices.GetService<IServiceProvider>())
            );

            await emptyResponseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            Serilog.Log.Logger.Error(ex, MessageConstant.ExceptionLogTemplate, ex.Message);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            await ExceptionMiddleware.RewriteResponseAsync(context, MessageKey.UnhandledException, (int)HttpStatusCode.BadRequest);
        }
    }

    private static async Task<string> ReadStreamInChunksAsync(RecyclableMemoryStream stream)
    {
        const int readChunkBufferLength = 4096;

        stream.Seek(0, SeekOrigin.Begin);

        using var textWriter = new StringWriter();

        using var reader = new StreamReader(stream);

        var readChunk = new char[readChunkBufferLength];

        int readChunkLength;

        do
        {
            readChunkLength = await reader.ReadBlockAsync(readChunk, 0, readChunkBufferLength);

            await textWriter.WriteAsync(readChunk, 0, readChunkLength);

        } while (readChunkLength > 0);

        return Regex.Unescape(textWriter.ToString());
    }

    private static string FilterBase64FromJson(JsonObject jsonObject)
    {
        // Özellikleri döngüye alıp base64 stringleri filtreleyin
        foreach (var property in jsonObject.ToList())
        {
            if (property.Value is JsonValue jsonValue && IsBase64String(jsonValue))
                jsonObject[property.Key] = "FILTERED";
            else if (property.Value is JsonObject jObj)
                FilterBase64FromJson(jObj);
            else if (property.Value is JsonArray jArray)
                foreach (var jNode in jArray)
                    FilterBase64FromJson(jNode.AsObject());
        }

        return jsonObject.ToString();
    }

    private static bool IsBase64String(JsonValue input)
    {
        try
        {
            var inputAsString = input.GetValue<string>();

            if (string.IsNullOrWhiteSpace(inputAsString) || inputAsString.Length % 4 != 0)
                return false;

            Convert.FromBase64String(inputAsString);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool Ignore(HttpContext httpContext)
    {
        if (IsUIRequest(httpContext)
            || httpContext.Request.Path.ToString().EndsWith("/download")
            || (httpContext.Request.ContentType?.Contains(MimeTypeNames.MultipartFormData, StringComparison.CurrentCultureIgnoreCase) ?? false))
            return true;

        return false;

        static bool IsUIRequest(HttpContext httpContext)
        {
            return httpContext.Request.Path.StartsWithSegments($"/{GlobalConstant.RoutePrefix}/documentation")
                    || httpContext.Request.Path.StartsWithSegments($"/{GlobalConstant.RoutePrefix}/docs")
                    || httpContext.Request.Path.StartsWithSegments($"/{GlobalConstant.RoutePrefix}/hc")
                    || httpContext.Request.Path.StartsWithSegments($"/{GlobalConstant.RoutePrefix}/health-check")
                    || httpContext.Request.Path.StartsWithSegments($"/{GlobalConstant.RoutePrefix}/hc-ui");
        }
    }

    private static bool IgnoreResponse(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(GlobalConstant.IgnoreResponseLoggingItemsKey, out var ignoreResponseLogging) && ignoreResponseLogging is bool ignoreResponse)
            return ignoreResponse;

        if (httpContext.Response.ContentType != null &&
             (httpContext.Response.ContentType.Contains(MimeTypeNames.ApplicationOctetStream, StringComparison.OrdinalIgnoreCase) ||
              httpContext.Response.ContentType.Contains(MimeTypeNames.MultipartFormData, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (httpContext.Response.Headers.TryGetValue("Content-Disposition", out StringValues value))
        {
            var contentDisposition = value.ToString();

            if (contentDisposition.Contains("attachment", StringComparison.OrdinalIgnoreCase) ||
                contentDisposition.Contains("inline", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}