namespace Milvonion.Application.Utils.Models;

/// <summary>
/// Settings of the application's Redis keys (configuration section <see cref="SectionName"/>).
/// </summary>
public class CacheKeyOptions
{
    /// <summary>
    /// Configuration section of the options.
    /// </summary>
    public const string SectionName = "CacheKeys";

    /// <summary>
    /// Prefix of every Redis key the application writes (the project name by default). Two applications that share one
    /// Redis (an API and its admin API, two environments on one server) must use different prefixes.
    /// </summary>
    public string Prefix { get; set; } = "milvonion";
}
