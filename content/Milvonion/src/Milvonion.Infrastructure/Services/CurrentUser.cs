using Microsoft.AspNetCore.Http;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Domain.Enums;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Reads the authenticated caller from the request's already-validated claims principal. No database
/// access and no token re-parsing: the JWT middleware has already validated the token and populated
/// <see cref="HttpContext.User"/>, so the id, user name and type are read straight from its claims.
/// </summary>
/// <param name="httpContextAccessor"></param>
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private HttpContext HttpContext => _httpContextAccessor.HttpContext;

    /// <inheritdoc/>
    public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <inheritdoc/>
    public int? Id
    {
        get
        {
            var raw = HttpContext?.User?.FindFirst(GlobalConstant.UserIdClaimName)?.Value;

            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public string UserName => HttpContext?.User?.Identity?.Name;

    /// <inheritdoc/>
    public UserType? UserType
    {
        get
        {
            var raw = HttpContext?.User?.FindFirst(GlobalConstant.UserTypeClaimName)?.Value;

            return Enum.TryParse<UserType>(raw, out var userType) ? userType : null;
        }
    }

    /// <inheritdoc/>
    public int GetRequiredId()
    {
        var id = Id;

        if (id is null)
            HttpContext.Response.ThrowWithUnauthorized(MessageKey.Unauthorized);

        return id.Value;
    }
}
