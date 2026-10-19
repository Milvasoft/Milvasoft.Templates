using FluentAssertions;
using Milvonion.Api.AppStartup;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// <see cref="MissingResxKeyFinder"/>: every localization key the application uses exists in every resx file.
/// </summary>
[Trait("Api Unit Tests", "Resx keys.")]
public class ResxTests
{
    [Fact]
    public void Find_ShouldReturnNoMissingKeys_ForAllResxFiles()
    {
        // Arrange & Act
        var missingKeys = MissingResxKeyFinder.Find();

        // Assert
        var report = string.Join(Environment.NewLine, missingKeys.Select(file => $"{file.Key}: {string.Join(", ", file.Value)}"));

        missingKeys.Should().BeEmpty($"every localization key must exist in every resx file, missing:{Environment.NewLine}{report}{Environment.NewLine}");
    }
}
