using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions.Localization;

namespace Milvonion.Application.Utils.Attributes;

/// <summary>
/// Determines the type of user who can access the method.
/// </summary>
/// <param name="userType"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class UserTypeAuthAttribute(UserType userType) : Attribute, IAuthorizationFilter
{
    private readonly UserType _userType = userType;

    /// <inheritdoc/>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var currentUserType = context.HttpContext.GetCurrentUserType();

        if (currentUserType == null || (_userType & currentUserType) != currentUserType)
        {
            // A result, not an exception: the envelope reaches the client like every other failure (403 + message).
            var localizer = context.HttpContext.RequestServices.GetService<IMilvaLocalizer>();

            var response = Response.Error(localizer != null ? localizer[MessageKey.Forbidden] : MessageKey.Forbidden, MessageType.Warning);
            response.StatusCode = StatusCodes.Status403Forbidden;

            context.Result = new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}