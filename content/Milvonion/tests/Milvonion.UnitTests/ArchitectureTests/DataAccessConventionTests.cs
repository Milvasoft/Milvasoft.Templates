using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Milvonion.UnitTests.TestSupport;
using System.Text.RegularExpressions;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Data access rules: no database round trip per loop iteration and projection on every read through the repository.
/// Database access is recognised syntactically by the receiver name (<c>*Repository</c>, <c>*DbContext</c>, <c>GetDbContext()</c>).
/// </summary>
[Trait("Architecture Tests", "Data access rules.")]
public partial class DataAccessConventionTests
{
    /// <summary>
    /// Repository read methods that accept a <c>projection</c> argument.
    /// </summary>
    private static readonly HashSet<string> _projectableReadMethods = ["GetAllAsync", "GetSomeAsync", "GetFirstOrDefaultAsync", "GetSingleOrDefaultAsync", "GetByIdAsync", "GetForDeleteAsync", "GetForUpdateAsync"];

    /// <summary>
    /// Reviewed places where a database call inside a loop is intentional. Key: "Project/RelativePath::Member".
    /// </summary>
    private static readonly HashSet<string> _allowedLoopDatabaseCalls =
    [
        // Development only seeding endpoints.
        "Milvonion.Infrastructure/Services/DeveloperService.cs::*",
    ];

    [Fact]
    public void DatabaseCalls_ShouldNotBeMadeInsideLoops()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .Where(IsLoop)
                                                                      .SelectMany(loop => LoopParts(loop).SelectMany(part => part.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()))
                                                                      .Where(IsDatabaseCall)
                                                                      .Where(invocation => !IsAllowedLoopDatabaseCall(file, invocation))
                                                                      .Select(invocation => $"{file.Location(invocation)}: {Shorten(invocation)}"));

        // Assert
        violations.ShouldBeEmpty("Database must not be called inside foreach/for/while loops, use bulk methods (ExecuteUpdateAsync, ExecuteDeleteAsync, BulkUpdateAsync, BulkAddAsync, list overloads)");
    }

    [Fact]
    public void RepositoryReadsUsedOnlyForNullCheck_ShouldUseProjection()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<InvocationExpressionSyntax>()
                                                                      .Where(invocation => IsRepositoryCall(invocation, out var method) && _projectableReadMethods.Contains(method))
                                                                      .Where(invocation => !HasProjection(invocation))
                                                                      .Where(IsOnlyNullChecked)
                                                                      .Select(invocation => $"{file.Location(invocation)}: {Shorten(invocation)}"));

        // Assert
        violations.ShouldBeEmpty("A read whose result is only null checked must project to the id (projection: e => new Entity {{ Id = e.Id }}) or use ExistsAsync");
    }

    /// <summary>
    /// True when the call has a <c>projection:</c> argument, or a positional argument that is a projection
    /// (<c>Entity.Projections.X</c> or a lambda creating an object).
    /// </summary>
    private static bool HasProjection(InvocationExpressionSyntax invocation)
        => invocation.ArgumentList.Arguments.Any(argument => argument.NameColon?.Name.Identifier.Text == "projection"
                                                             || (argument.NameColon == null && argument.Expression.ToString().Contains("Projection"))
                                                             || (argument.NameColon == null && argument.Expression is LambdaExpressionSyntax { Body: BaseObjectCreationExpressionSyntax }));

    /// <summary>
    /// True when the awaited result is compared with null directly (<c>await repo.GetX() != null</c>) or stored in a local
    /// that is used nowhere else than in null checks.
    /// </summary>
    private static bool IsOnlyNullChecked(InvocationExpressionSyntax invocation)
    {
        if (invocation.Parent is not AwaitExpressionSyntax awaitExpression)
            return false;

        if (IsNullCheck(awaitExpression))
            return true;

        if (awaitExpression.Parent is not EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator })
            return false;

        var scope = declarator.FirstAncestorOrSelf<SyntaxNode>(n => n is BaseMethodDeclarationSyntax or LocalFunctionStatementSyntax or AccessorDeclarationSyntax);

        if (scope == null)
            return false;

        var usages = scope.DescendantNodes()
                          .OfType<IdentifierNameSyntax>()
                          .Where(identifier => identifier.Identifier.Text == declarator.Identifier.Text && !IsShadowedByLambdaParameter(identifier))
                          .ToList();

        return usages.Count > 0 && usages.All(IsNullCheck);
    }

    private static bool IsShadowedByLambdaParameter(IdentifierNameSyntax identifier)
        => identifier.Ancestors()
                     .OfType<LambdaExpressionSyntax>()
                     .Any(lambda => lambda switch
                     {
                         SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text == identifier.Identifier.Text,
                         ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters.Any(p => p.Identifier.Text == identifier.Identifier.Text),
                         _ => false,
                     });

    private static bool IsNullCheck(ExpressionSyntax expression) => expression.Parent switch
    {
        BinaryExpressionSyntax binary => (binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression))
                                         && (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) || binary.Right.IsKind(SyntaxKind.NullLiteralExpression)),
        IsPatternExpressionSyntax pattern => pattern.Pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal } && literal.IsKind(SyntaxKind.NullLiteralExpression)
                                             || pattern.Pattern is UnaryPatternSyntax { Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax notLiteral } } && notLiteral.IsKind(SyntaxKind.NullLiteralExpression),
        ParenthesizedExpressionSyntax parenthesized => IsNullCheck(parenthesized),
        _ => false,
    };

    private static bool IsAllowedLoopDatabaseCall(SourceFile file, SyntaxNode node)
    {
        var member = node.FirstAncestorOrSelf<MemberDeclarationSyntax>(m => m is BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax);
        var memberName = member is MethodDeclarationSyntax method ? method.Identifier.Text : member?.Kind().ToString();
        var fileKey = $"{file.Project}/{file.RelativePath}";

        return _allowedLoopDatabaseCalls.Contains($"{fileKey}::*") || _allowedLoopDatabaseCalls.Contains($"{fileKey}::{memberName}");
    }

    private static bool IsLoop(SyntaxNode node) => node is CommonForEachStatementSyntax or ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax;

    /// <summary>
    /// Parts of a loop that run on every iteration: the body and the condition / incrementors.
    /// </summary>
    private static IEnumerable<SyntaxNode> LoopParts(SyntaxNode loop) => loop switch
    {
        CommonForEachStatementSyntax forEach => [forEach.Statement],
        ForStatementSyntax @for => [@for.Statement, .. @for.Incrementors, .. OrEmpty(@for.Condition)],
        WhileStatementSyntax @while => [@while.Statement, @while.Condition],
        DoStatementSyntax @do => [@do.Statement, @do.Condition],
        _ => throw new ArgumentOutOfRangeException(nameof(loop)),
    };

    private static IEnumerable<SyntaxNode> OrEmpty(SyntaxNode node) => node == null ? [] : [node];

    private static bool IsDatabaseCall(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || !memberAccess.Name.Identifier.Text.EndsWith("Async"))
            return false;

        return IsDatabaseReceiver(memberAccess.Expression);
    }

    private static bool IsRepositoryCall(InvocationExpressionSyntax invocation, out string method)
    {
        method = null;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        method = memberAccess.Name.Identifier.Text;

        return RepositoryName().IsMatch(RootIdentifier(memberAccess.Expression) ?? string.Empty);
    }

    /// <summary>
    /// Walks the receiver chain (<c>_dbContext.Set&lt;T&gt;().Where(...)</c>) looking for a repository or db context.
    /// </summary>
    private static bool IsDatabaseReceiver(ExpressionSyntax expression)
    {
        for (var current = expression; current != null;)
        {
            switch (current)
            {
                case IdentifierNameSyntax identifier:
                    return DatabaseName().IsMatch(identifier.Identifier.Text);
                case MemberAccessExpressionSyntax memberAccess:
                    if (DatabaseName().IsMatch(memberAccess.Name.Identifier.Text))
                        return true;

                    current = memberAccess.Expression;
                    break;
                case InvocationExpressionSyntax nested:
                    if (nested.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "GetDbContext" })
                        return true;

                    current = nested.Expression;
                    break;
                case ParenthesizedExpressionSyntax parenthesized:
                    current = parenthesized.Expression;
                    break;
                default:
                    return false;
            }
        }

        return false;
    }

    private static string RootIdentifier(ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.Text,
        MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess => memberAccess.Name.Identifier.Text,
        _ => null,
    };

    private static string Shorten(SyntaxNode node)
    {
        var text = Regex.Replace(node.ToString(), @"\s+", " ");

        return text.Length > 120 ? text[..120] + "..." : text;
    }

    [GeneratedRegex("(Repository|DbContext)$", RegexOptions.IgnoreCase)]
    private static partial Regex DatabaseName();

    [GeneratedRegex("Repository$", RegexOptions.IgnoreCase)]
    private static partial Regex RepositoryName();
}
