using System.Reflection;

namespace Milvonion.Application;

/// <summary>
/// Static class for ease of assembly access.
/// </summary>
public static class ApplicationAssembly
{
    /// <summary>
    /// Assembly instance.
    /// </summary>
    public static readonly Assembly Assembly = typeof(ApplicationAssembly).Assembly;
}
