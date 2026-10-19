using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Components.Rest.MilvaResponse;

namespace Milvonion.Api.Utils;

/// <summary>
/// Global result filter that sends failed envelopes with a real HTTP status code instead of 200. Handlers
/// keep returning <see cref="IResponse"/> with <c>IsSuccess=false</c>; this filter reads the message key
/// (see <see cref="ResponseStatusCodes"/>) and sets both the wire status and the envelope's
/// <see cref="IResponse.StatusCode"/> so the two never disagree. Results that already carry an explicit
/// non-200 status are left untouched.
/// </summary>
public sealed class ResponseStatusCodeFilter : IAsyncResultFilter
{
    /// <inheritdoc/>
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: IResponse response } objectResult && objectResult.StatusCode is null or StatusCodes.Status200OK)
        {
            var status = ResponseStatusCodes.FromResponse(response);

            if (status != StatusCodes.Status200OK)
            {
                response.StatusCode = status;
                objectResult.StatusCode = status;
            }
        }

        return next();
    }
}
