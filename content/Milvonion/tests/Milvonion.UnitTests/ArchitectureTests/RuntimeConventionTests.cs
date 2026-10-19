using Microsoft.CodeAnalysis.CSharp.Syntax;
using Milvonion.UnitTests.TestSupport;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Runtime behaviour rules that are visible in the source: the product reads one clock (UTC) and writes to one
/// log (ILogger / IMilvaLogger). Both are invisible in a review and only show up on a server in another time
/// zone, or as a message nobody ever sees because it went to a console that does not exist in production.
/// </summary>
[Trait("Architecture Tests", "Runtime behaviour rules.")]
public class RuntimeConventionTests
{
    /// <summary>
    /// Places where a local clock is intended. Empty on purpose: every time is UTC (export file names and seed dates
    /// included). Adding a file here is a decision, not a formality.
    /// </summary>
    private static readonly HashSet<string> _localClockAllowedFiles = [];

    /// <summary>
    /// The only place outside AppStartup that may talk to a terminal: the development only tooling endpoints.
    /// </summary>
    private static readonly HashSet<string> _consoleAllowedFiles =
    [
        "Milvonion.Infrastructure/Services/DeveloperService.cs",
    ];

    /// <summary>
    /// The composition root folder, which writes before the logger exists and in the top level catch after it is gone.
    /// </summary>
    private const string _startupFolder = "AppStartup";

    [Fact]
    public void Code_ShouldNotReadTheLocalClock()
    {
        // Act: DateTime.Now changes meaning with the server's time zone, and Npgsql refuses to write a DateTimeOffset.Now
        // with a non zero offset to a timestamptz column, so neither is used.
        var violations = SourceCode.Files.Where(file => !_localClockAllowedFiles.Contains($"{file.Project}/{file.RelativePath}"))
                                         .SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<MemberAccessExpressionSyntax>()
                                                                      .Where(access => access.Expression.ToString() is nameof(DateTime) or nameof(DateTimeOffset))
                                                                      .Where(access => access.Name.Identifier.Text == nameof(DateTime.Now))
                                                                      .Select(access => $"{file.Location(access)}: {access}"));

        // Assert
        violations.ShouldBeEmpty("The database stores UTC and the jobs compare against now(); use DateTime.UtcNow / DateTimeOffset.UtcNow");
    }

    [Fact]
    public void Code_ShouldNotWriteToTheConsole()
    {
        // Act
        var violations = SourceCode.Files.Where(file => !_consoleAllowedFiles.Contains($"{file.Project}/{file.RelativePath}") && !file.Folder.Split('/').Contains(_startupFolder))
                                         .SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<MemberAccessExpressionSyntax>()
                                                                      .Where(access => access.Expression.ToString() == nameof(Console))
                                                                      .Select(access => $"{file.Location(access)}: {access}"));

        // Assert
        violations.ShouldBeEmpty("Logging goes through ILogger / IMilvaLogger so it reaches the sinks; the console reaches nothing in production");
    }
}
