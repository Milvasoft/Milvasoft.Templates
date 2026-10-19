using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Milvasoft.Core.Utils.Constants;
using Milvonion.Application.Utils.Constants;
using Milvonion.UnitTests.TestSupport;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// User facing texts (mail, notification, SMS...) are never hard coded, they live in the resx files and are resolved
/// through IMilvaLocalizer. Detected by Turkish characters in string literals, since hard coded texts are written in Turkish.
/// </summary>
[Trait("Architecture Tests", "Localization rules.")]
public partial class LocalizationConventionTests
{
    /// <summary>
    /// Files whose Turkish literals are data, not user facing messages.
    /// </summary>
    private static readonly HashSet<string> _allowedFiles =
    [
        // OpenAPI example values.
        "Milvonion.Infrastructure/Utils/OpenApi/ExampleSchemaTransformer.cs",
    ];

    [Fact]
    public void StringLiterals_ShouldNotContainHardCodedTurkishText()
    {
        // Act
        var violations = SourceCode.Files.Where(file => !_allowedFiles.Contains($"{file.Project}/{file.RelativePath}"))
                                         .SelectMany(file => file.Root.DescendantTokens()
                                                                      .Where(IsStringToken)
                                                                      .Where(token => TurkishCharacters().IsMatch(token.Text))
                                                                      .Select(token => $"{file.Location(token)}: {token.Text}"));

        // Assert
        violations.ShouldBeEmpty("User facing texts must be added to MessageKey + SharedResource.tr-TR/en-US.resx and resolved with IMilvaLocalizer");
    }

    [Fact]
    public void ResxKeys_ShouldNotContainOrphanedMessageKeys()
    {
        // The other direction of the resx contract (MissingResxKeyFinder checks that every key the code asks
        // for exists): a key whose MessageKey constant was deleted while its text stayed behind is dead weight
        // nobody notices. Limited to the "...NotFound" / "...Exists" families the handlers answer with, and a
        // key the code still asks for by name (localizer["Exists"] in ExistsNotFormatter) counts as in use.
        // Milvasoft's own LocalizerKeys (e.g. FileNotFound) are asked for by the framework, so they are declared too.
        var declared = new[] { typeof(MessageKey), typeof(LocalizerKeys) }.SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                                                                          .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                                                                          .Select(field => field.GetRawConstantValue().ToString())
                                                                          .ToHashSet(StringComparer.Ordinal);

        // Act
        var violations = ResxKeys().Where(key => key.EndsWith("NotFound", StringComparison.Ordinal) || key.EndsWith("Exists", StringComparison.Ordinal))
                                   .Where(key => !declared.Contains(key) && !SourceCode.Files.Any(file => file.Text.Contains($"[\"{key}\"]", StringComparison.Ordinal)))
                                   .Select(key => $"SharedResource.tr-TR.resx: {key}");

        // Assert
        violations.ShouldBeEmpty("A resx key nobody asks for must be removed from both cultures");
    }

    /// <summary>
    /// Data names declared in the API's Turkish shared resource file (both cultures carry the same keys).
    /// </summary>
    private static IEnumerable<string> ResxKeys()
    {
        // Touch the sources first, so a run without them checked out is skipped instead of throwing.
        _ = SourceCode.Files;

        var path = Path.Combine(SourceCode.SolutionRoot, "src", "Milvonion.Api", "LocalizationResources", "Resources", "SharedResource.tr-TR.resx");

        return XDocument.Load(path).Root.Elements("data").Select(element => element.Attribute("name").Value);
    }

    private static bool IsStringToken(SyntaxToken token)
        => token.IsKind(SyntaxKind.StringLiteralToken)
           || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
           || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
           || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
           || token.IsKind(SyntaxKind.Utf8StringLiteralToken);

    [GeneratedRegex("[ğĞşŞıİçÇöÖüÜ]")]
    private static partial Regex TurkishCharacters();
}
