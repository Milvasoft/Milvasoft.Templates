using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;

namespace Milvonion.Api.Utils;

/// <summary>
/// Server-side kill switch for endpoints whose feature an admin can switch off (an expensive operation, a paid
/// provider). When the flag is off (see FeatureFlagsController), the request is answered with HTTP 503 before the
/// action runs, so an old client or a direct API call cannot keep using it. Fails OPEN: a missing row or a failed
/// read lets the request through (see <see cref="IFeatureFlagService"/>).
/// </summary>
/// <param name="key">One of <see cref="FeatureFlagKeys"/>.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class FeatureFlagAttribute(string key) : ActionFilterAttribute
{
    private readonly string _key = key;

    /// <inheritdoc/>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var featureFlagService = http.RequestServices.GetService<IFeatureFlagService>();

        if (featureFlagService == null || await featureFlagService.IsEnabledAsync(_key, http.RequestAborted))
        {
            await next();
            return;
        }

        var localizer = http.RequestServices.GetService<IMilvaLocalizer>();

        var response = Response.Error(localizer != null ? localizer[MessageKey.FeatureDisabled] : MessageKey.FeatureDisabled, MessageType.Warning);
        response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        context.Result = new ObjectResult(response) { StatusCode = StatusCodes.Status503ServiceUnavailable };
    }
}
