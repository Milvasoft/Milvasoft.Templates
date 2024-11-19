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
    /// Health check path. 
    /// </summary>
    public const string HealthCheckPath = "hc";

    /// <summary>
    /// Health check ui path. 
    /// </summary>
    public const string HealthCheckUIPath = "hc-ui";

    /// <summary>
    /// Health check ui path. 
    /// </summary>
    public const string HealthCheckWebHookPath = "hc-ui";

    /// <summary>
    /// Health check resources path. 
    /// </summary>
    public const string HealthCheckResourcesPath = "health-check";

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
    /// Default api version.
    /// </summary>
    public const string DefaultApiVersion = "v1.0";

    /// <summary>
    /// Custom forbid scheme for super admin.
    /// </summary>
    public const string CustomForbidSchema = "CustomForbidSchema";

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
    /// Response logging ignore httpcontext items key.
    /// </summary>
    public const string IgnoreResponseLoggingItemsKey = "IgnoreResponseLogging";

    /// <summary>
    /// Root user name.
    /// </summary>
    public const string RootUsername = "rootuser";
}

