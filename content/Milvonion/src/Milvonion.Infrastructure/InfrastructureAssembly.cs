﻿using System.Reflection;

namespace Milvonion.Infrastructure;

/// <summary>
/// Static class for ease of assembly access.
/// </summary>
public static class InfrastructureAssembly
{
    /// <summary>
    /// Assembly instance.
    /// </summary>
    public static readonly Assembly Assembly = typeof(InfrastructureAssembly).Assembly;
}
