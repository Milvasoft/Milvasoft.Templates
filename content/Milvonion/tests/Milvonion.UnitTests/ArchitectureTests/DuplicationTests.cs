using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Milvonion.UnitTests.TestSupport;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Copy/paste detection. Logic repeated in several handlers or services must live in a shared helper or extension
/// (e.g. Utils/Extensions/RepositoryExtensions.cs).
/// </summary>
[Trait("Architecture Tests", "Duplication rules.")]
public class DuplicationTests
{
    /// <summary>
    /// Bodies shorter than this many tokens (trivial guards, one line delegations) are ignored.
    /// </summary>
    private const int _minimumTokenCount = 40;

    [Fact]
    public void Methods_ShouldNotBeDuplicatedAcrossTypes()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<BaseMethodDeclarationSyntax>()
                                                                      .Select(method => (file, method, body: NormalizedBody(method))))
                                         .Where(m => m.body != null)
                                         .GroupBy(m => m.body)
                                         .Where(g => g.Select(m => TypeName(m.method)).Distinct().Count() > 1)
                                         .Select(g => string.Join(" == ", g.Select(m => $"{m.file.Location(m.method)} {TypeName(m.method)}.{MethodName(m.method)}")));

        // Assert
        violations.ShouldBeEmpty("Identical method bodies must be moved into a shared helper/extension");
    }

    /// <summary>
    /// Returns the body tokens joined by a space (formatting and comments ignored), or null for short bodies.
    /// </summary>
    private static string NormalizedBody(BaseMethodDeclarationSyntax method)
    {
        SyntaxNode body = (SyntaxNode)method.Body ?? method.ExpressionBody;

        if (body == null)
            return null;

        var tokens = body.DescendantTokens().Select(t => t.Text).ToList();

        return tokens.Count < _minimumTokenCount ? null : string.Join(' ', tokens);
    }

    private static string TypeName(SyntaxNode node) => node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>()?.Identifier.Text;

    private static string MethodName(BaseMethodDeclarationSyntax method) => method switch
    {
        MethodDeclarationSyntax named => named.Identifier.Text,
        ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
        _ => method.Kind().ToString(),
    };
}
