using System.Reflection;

namespace Milvonion.Api;

/// <summary>
/// Static class for ease of assembly access.
/// </summary>
public static class PresentationAssembly
{
    /// <summary>
    /// Assembly instance.
    /// </summary>
    public static readonly Assembly Assembly = typeof(PresentationAssembly).Assembly;
}
