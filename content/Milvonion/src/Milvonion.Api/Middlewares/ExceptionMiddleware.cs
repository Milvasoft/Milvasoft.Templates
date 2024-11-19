using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvonion.Application.Utils.Constants;
using Npgsql;
using System.Net;

namespace Milvonion.Api.Middlewares;

/// <summary>
/// Catches errors occurring elsewhere in the project.
/// </summary>
/// <remarks>
/// Constructor of <see cref="ExceptionMiddleware"/> class.
/// </remarks>
/// <param name="next"></param>
/// <param name="loggerFactory"></param>
public class ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = loggerFactory.CreateLogger<ExceptionMiddleware>();

    /// <summary>
    /// Invokes the method or constructor reflected by this MethodInfo instance.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            //Continue processing
            if (_next != null && !context.Response.HasStarted)
                await _next.Invoke(context);
        }
        catch (PostgresException ex)
        {
            context.Items.Add(nameof(Exception), ex);

            await HandlePostgresException(context, ex);
        }
        catch (MilvaUserFriendlyException ex)
        {
            context.Items.Add(nameof(Exception), ex);

            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized || context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                await RewriteResponseAsync(context, context.Response.StatusCode == (int)HttpStatusCode.Unauthorized ? MessageKey.Unauthorized : MessageKey.Forbidden, context.Response.StatusCode, MessageType.Warning);
                return;
            }

            await RewriteResponseAsync(context, ex.Message, (int)HttpStatusCode.OK, MessageType.Warning);
        }
        catch (Exception ex)
        {
            context.Items.Add(nameof(Exception), ex);

            if (ex.InnerException is PostgresException postgresEx)
                await HandlePostgresException(context, postgresEx);
            else
            {
                _logger.LogError(ex, MessageConstant.ExceptionLogTemplate, ex.Message);

                await RewriteResponseAsync(context, MessageKey.UnhandledException, (int)HttpStatusCode.BadRequest);
            }
        }
    }

    private static async Task HandlePostgresException(HttpContext context, PostgresException ex)
    {
        string messageKey;

        if (ex.SqlState.Equals(MessageConstant.DefaultDataCannotModifyPgCode) && ex.Message.Equals(MessageConstant.DefaultDataCannotModifyPgMessage))
            messageKey = MessageKey.DefaultValueCannotModify;
        else if (ex.SqlState.Equals(MessageConstant.DuplicateDataViolationPgCode))
            messageKey = MessageKey.PostgreDuplicateDataException;
        else if (ex.SqlState.Equals(MessageConstant.ForeignKeyViolationPgCode))
            messageKey = MessageKey.PostgreBasedException;
        else
            messageKey = MessageKey.PostgreBasedException;

        await RewriteResponseAsync(context, messageKey, (int)HttpStatusCode.OK, MessageType.Warning);
    }

    /// <summary>
    /// Rewrites the response.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="messageKey"></param>
    /// <param name="statusCode"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public static async Task RewriteResponseAsync(HttpContext context, string messageKey, int statusCode, MessageType messageType = MessageType.Error)
    {
        if (!context.Response.HasStarted)
        {
            var localizer = context.RequestServices.GetService<IMilvaLocalizer>();

            var response = Response.Error(localizer[messageKey], messageType);

            response.StatusCode = statusCode;
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
