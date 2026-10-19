using Microsoft.AspNetCore.Http;

namespace Milvonion.Application.Utils.Extensions;

/// <summary>
/// Stashes and reads the "related record" of the current request so the <see cref="UserActivityLogInterceptor"/> can
/// attach it to the activity log. A handler calls <see cref="SetUserActivityRelated"/> with the affected record's id
/// (and optional JSON detail); the interceptor reads it after the handler succeeds. Stored in <see cref="HttpContext.Items"/>,
/// so it lives for the request only.
/// </summary>
public static class UserActivityContextExtensions
{
    private const string _relatedIdKey = "UserActivity.RelatedId";
    private const string _relatedDataKey = "UserActivity.RelatedData";

    /// <summary>
    /// Records the record this request acted on (id, and optionally a small JSON detail) for the activity log.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="relatedId"></param>
    /// <param name="relatedData"></param>
    public static void SetUserActivityRelated(this IHttpContextAccessor httpContextAccessor, object relatedId, string relatedData = null)
    {
        var items = httpContextAccessor?.HttpContext?.Items;

        if (items == null)
            return;

        if (relatedId != null)
            items[_relatedIdKey] = relatedId.ToString();

        if (relatedData != null)
            items[_relatedDataKey] = relatedData;
    }

    /// <summary>
    /// The related record id set by the handler for the current request, or null.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static string GetUserActivityRelatedId(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor?.HttpContext?.Items.TryGetValue(_relatedIdKey, out var value) == true ? value as string : null;

    /// <summary>
    /// The related detail (JSON) set by the handler for the current request, or null.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static string GetUserActivityRelatedData(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor?.HttpContext?.Items.TryGetValue(_relatedDataKey, out var value) == true ? value as string : null;
}
