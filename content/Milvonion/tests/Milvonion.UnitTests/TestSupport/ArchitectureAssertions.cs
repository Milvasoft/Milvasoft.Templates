using FluentAssertions;

namespace Milvonion.UnitTests.TestSupport;

/// <summary>
/// Assertion helpers for architecture tests, so a failing rule lists every offending location at once.
/// </summary>
public static class ArchitectureAssertions
{
    /// <summary>
    /// Asserts that no violation was found, printing all violations (one per line) otherwise.
    /// </summary>
    /// <param name="violations">Violation descriptions, usually "location: detail".</param>
    /// <param name="rule">Human readable rule that was checked.</param>
    public static void ShouldBeEmpty(this IEnumerable<string> violations, string rule)
    {
        var list = violations.Distinct().OrderBy(v => v, StringComparer.Ordinal).ToList();

        list.Should().BeEmpty("{0}. {1} violation(s):{2}{3}", rule, list.Count, Environment.NewLine, string.Join(Environment.NewLine, list));
    }
}
