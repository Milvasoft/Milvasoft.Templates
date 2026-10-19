using System.Diagnostics;

namespace Milvonion.Application.Utils.Constants;

/// <summary>
/// Represents a class that contains global constants for the application.
/// </summary>
public static class GlobalConstant
{
    #region Path

    /// <summary>
    /// Route prefix of api.
    /// </summary>
    public const string RoutePrefix = "xrouteprefixx";

    /// <summary>
    /// Base route path of api.
    /// </summary>
    public const string RouteBase = RoutePrefix + "/" + "v{version:apiVersion}";

    /// <summary>
    /// Full route path of api. It includes <see cref="RouteBase"/> and controller name. 
    /// </summary>
    public const string FullRoute = $"{RouteBase}/[controller]";

    /// <summary>
    /// Root directory path. 
    /// </summary>
    public const string WWWRoot = "wwwroot";

    /// <summary>
    /// http string.
    /// </summary>
    public const string Http = "http";

    /// <summary>
    /// https string.
    /// </summary>
    public const string Https = "https";

    /// <summary>
    /// Localization resources folder name.
    /// </summary>
    public const string LocalizationResourcesFolderName = "LocalizationResources";

    /// <summary>
    /// Resources folder name.
    /// </summary>
    public const string ResourcesFolderName = "Resources";

    #endregion

    /// <summary>
    /// Rootpath of application.  
    /// </summary>
    public static string RootPath { get; } = Environment.CurrentDirectory;

    /// <summary>
    /// SQL folder path.  
    /// </summary>
    public static string SqlFilesPath { get; } = Path.Combine(RootPath, "StaticFiles", "SQL");

    /// <summary>
    /// SQL folder path.  
    /// </summary>
    public static string JsonFilesPath { get; } = Path.Combine(RootPath, "StaticFiles", "JSON");

    /// <summary>
    /// Default api version.
    /// </summary>
    public const string DefaultApiVersion = "v1.0";

    /// <summary>
    /// Default api version.
    /// </summary>
    public const string CurrentApiVersion = "1.0";

    /// <summary>
    /// Custom forbid scheme for super admin.
    /// </summary>
    public const string CustomForbidSchema = "CustomForbidSchema";

    /// <summary>
    /// Activity source of the application traces. Its name is also the default OpenTelemetry service name.
    /// </summary>
    public static ActivitySource ActivitySource { get; } = new("milvonion-api");

    /// <summary>
    /// Logging activity name for activity starter interception.
    /// </summary>
    public const string LoggingActivityName = "LoggingActivity";

    /// <summary>
    /// Gets or sets the current environment.
    /// </summary>
    public static string CurrentEnvironment { get; set; }

    /// <summary>
    /// UserType claim name.
    /// </summary>
    public const string UserTypeClaimName = "ut";

    /// <summary>
    /// User id claim name. Carried in the access token so the current user's id is read straight from
    /// the (already validated) claims, without a per-request "resolve id from user name" database round trip.
    /// </summary>
    public const string UserIdClaimName = "uid";

    /// <summary>
    /// Session (revocation) stamp claim name. Baked into the access token at login; every request
    /// compares it against the user's current stamp (Redis, DB fallback). A mismatch means the token
    /// was revoked (password change, role change, lock, ...) and the request is rejected.
    /// </summary>
    public const string SessionStampClaimName = "sstamp";

    /// <summary>
    /// Root user name.
    /// </summary>
    public const string RootUsername = "rootuser";

    /// <summary>
    /// System user name.
    /// </summary>
    public const string SystemUsername = "System";

    /// <summary>
    /// Generate metadata header key.
    /// </summary>
    public const string GenerateMetadataHeaderKey = "M-Metadata";

    /// <summary>
    /// Generate metadata header key.
    /// </summary>
    public const string ContentDispositionHeaderKey = "Content-Disposition";

    /// <summary>
    /// Generate metadata header key.
    /// </summary>
    public const string DownloadEnpointPathEnd = "/download";

    /// <summary>
    /// Default ip.
    /// </summary>
    public const string DefaultIp = "0.0.0.1";

    /// <summary>
    /// Url start segment.
    /// </summary>
    public const string UrlStartSegment = "://";

    /// <summary>
    /// Auto increment start value for database entities.
    /// </summary>
    public const int AutoIncrementStart = 21;

    /// <summary>
    /// Smallest page a list endpoint serves.
    /// </summary>
    public const int MinRowCount = 1;

    /// <summary>
    /// Largest page a list endpoint serves; a larger client RowCount is clamped to it.
    /// </summary>
    public const int MaxRowCount = 100;

    /// <summary>
    /// Upper bound of rows a single export may pull; also the page cap of the list queries an export runs.
    /// </summary>
    public const int MaxExportRowCount = 5000;

    /// <summary>
    /// Ignored content disposition parts.
    /// </summary>
    public static HashSet<string> ContentDispositionIgnores { get; } =
    [
        "attachment",
        "inline"
    ];

    /// <summary>
    /// UI request paths.
    /// </summary>
    public static HashSet<string> UIPaths { get; } =
    [
        $"/{RoutePrefix}/documentation",
        $"/{RoutePrefix}/docs"
    ];
}

