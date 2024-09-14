using System.Reflection;

namespace Milvonion.Domain;

/// <summary>
/// Static class for ease of assembly access.
/// </summary>
public static class DomainAssembly
{
    /// <summary>
    /// Assembly instance.
    /// </summary>
    public static readonly Assembly Assembly = typeof(DomainAssembly).Assembly;
}
