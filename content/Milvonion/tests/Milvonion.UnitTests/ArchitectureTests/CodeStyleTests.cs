using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Milvonion.UnitTests.TestSupport;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Source level code standards of the src projects (formatting, naming, file layout).
/// Uses the Roslyn syntax tree so comments and layout are checked as written.
/// </summary>
[Trait("Architecture Tests", "Code style rules.")]
public class CodeStyleTests
{
    private const char _emDash = '\u2014';
    private const char _enDash = '\u2013';
    private const string _missingXmlCommentWarning = "CS1591";

    /// <summary>
    /// Files whose namespace or name is fixed by a framework convention.
    /// </summary>
    private static readonly HashSet<string> _namespaceAndFileNameExemptFiles =
    [
        // Resx lookup uses the Milvonion.Api.SharedResource type name.
        "Milvonion.Api/LocalizationResources/SharedResource.cs",
    ];

    /// <summary>
    /// Framework contract methods whose names are fixed by the implemented interface or convention.
    /// </summary>
    private static readonly HashSet<string> _asyncSuffixExemptMethods = ["Handle", "Invoke", "Main", "OnInvoke"];

    [Fact]
    public void Sources_ShouldNotContainEmDash()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Text.Split('\n')
                                                                      .Select((line, index) => (line, index))
                                                                      .Where(l => l.line.Contains(_emDash))
                                                                      .Select(l => $"{file.Location(l.index)}: {l.line.Trim()}"));

        // Assert
        violations.ShouldBeEmpty("Em dash must not be used in code, comments or strings");
    }

    [Fact]
    public void Sources_ShouldNotContainEnDash()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Text.Split('\n')
                                                                      .Select((line, index) => (line, index))
                                                                      .Where(l => l.line.Contains(_enDash))
                                                                      .Select(l => $"{file.Location(l.index)}: {l.line.Trim()}"));

        // Assert
        violations.ShouldBeEmpty("En dash must not be used in code, comments or strings");
    }

    [Fact]
    public void IfStatements_ShouldNotBeWrittenOnOneLine()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<IfStatementSyntax>()
                                                                      .Where(node => IsOnSameLine(node.CloseParenToken, node.Statement))
                                                                      .Select(node => $"{file.Location(node)}: {node.ToString().Split('\n')[0].Trim()}"));

        var elseViolations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                          .OfType<ElseClauseSyntax>()
                                                                          .Where(node => node.Statement is not IfStatementSyntax && IsOnSameLine(node.ElseKeyword, node.Statement))
                                                                          .Select(node => $"{file.Location(node)}: {node.ToString().Split('\n')[0].Trim()}"));

        // Assert
        violations.Concat(elseViolations).ShouldBeEmpty("if/else bodies must start on their own line (or be a block)");
    }

    [Fact]
    public void Files_ShouldUseFileScopedNamespace()
    {
        // Act
        var violations = SourceCode.Files.Where(file => file.Root.Members.OfType<NamespaceDeclarationSyntax>().Any())
                                         .Select(file => $"{file.Location(0)}: block scoped namespace");

        // Assert
        violations.ShouldBeEmpty("Files must use file scoped namespaces");
    }

    [Fact]
    public void Files_ShouldIndentWithSpaces()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Text.Split('\n')
                                                                      .Select((line, index) => (line, index))
                                                                      .Where(l => l.line.StartsWith('\t'))
                                                                      .Select(l => $"{file.Location(l.index)}: tab indentation"));

        // Assert
        violations.ShouldBeEmpty("Indentation is 4 spaces, per the .editorconfig");
    }

    [Fact]
    public void Namespaces_ShouldMatchFolderStructure()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.Members
                                                                      .OfType<BaseNamespaceDeclarationSyntax>()
                                                                      .Where(ns => ns.Name.ToString() != ExpectedNamespace(file))
                                                                      .Where(_ => !_namespaceAndFileNameExemptFiles.Contains($"{file.Project}/{file.RelativePath}"))
                                                                      .Select(ns => $"{file.Location(ns)}: namespace {ns.Name} (expected {ExpectedNamespace(file)})"));

        // Assert
        violations.ShouldBeEmpty("Namespaces must match the folder structure");
    }

    [Fact]
    public void SingleTypeFiles_ShouldBeNamedAfterTheirType()
    {
        // Act
        var violations = SourceCode.Files.Where(file => TopLevelTypes(file).Count() == 1)
                                         .Select(file => (file, type: TopLevelTypes(file).Single()))
                                         .Where(t => !t.type.Modifiers.Any(SyntaxKind.PartialKeyword))
                                         .Where(t => !_namespaceAndFileNameExemptFiles.Contains($"{t.file.Project}/{t.file.RelativePath}"))
                                         .Where(t => TypeName(t.type) != t.file.Name.TrimStart('_'))
                                         .Select(t => $"{t.file.Location(0)}: declares {TypeName(t.type)}");

        // Assert
        violations.ShouldBeEmpty("A file with a single (non partial) type must be named after that type");
    }

    [Fact]
    public void Types_ShouldBePascalCaseAndInterfacesShouldStartWithI()
    {
        // Act
        var types = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Select(type => (file, type)));

        var notPascal = types.Where(t => !IsPascalCase(t.type.Identifier.Text))
                             .Select(t => $"{t.file.Location(t.type)}: {t.type.Identifier.Text} is not PascalCase");

        var badInterfaces = types.Where(t => t.type is InterfaceDeclarationSyntax)
                                 .Where(t => t.type.Identifier.Text.Length < 2 || t.type.Identifier.Text[0] != 'I' || !char.IsUpper(t.type.Identifier.Text[1]))
                                 .Select(t => $"{t.file.Location(t.type)}: interface {t.type.Identifier.Text} must start with I");

        // Assert
        notPascal.Concat(badInterfaces).ShouldBeEmpty("Type naming rules");
    }

    [Fact]
    public void Methods_ShouldBePascalCase()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<MethodDeclarationSyntax>()
                                                                      .Where(method => !IsPascalCase(method.Identifier.Text))
                                                                      .Select(method => $"{file.Location(method)}: {method.Identifier.Text}"));

        // Assert
        violations.ShouldBeEmpty("Method names must be PascalCase");
    }

    [Fact]
    public void PrivateFields_ShouldBeUnderscoreCamelCase()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<FieldDeclarationSyntax>()
                                                                      .Where(field => IsPrivateOrInternal(field.Modifiers) && field.Parent is not EnumDeclarationSyntax)
                                                                      .SelectMany(field => field.Declaration.Variables)
                                                                      .Where(variable => !IsUnderscoreCamelCase(variable.Identifier.Text))
                                                                      .Select(variable => $"{file.Location(variable)}: {variable.Identifier.Text}"));

        // Assert
        violations.ShouldBeEmpty("Private/internal fields must be named _camelCase");
    }

    [Fact]
    public void InstanceFields_ShouldNotBePublic()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<FieldDeclarationSyntax>()
                                                                      .Where(field => field.Modifiers.Any(SyntaxKind.PublicKeyword)
                                                                                      && !field.Modifiers.Any(SyntaxKind.ConstKeyword)
                                                                                      && !field.Modifiers.Any(SyntaxKind.StaticKeyword)
                                                                                      && field.Parent is not StructDeclarationSyntax)
                                                                      .SelectMany(field => field.Declaration.Variables)
                                                                      .Select(variable => $"{file.Location(variable)}: {variable.Identifier.Text}"));

        // Assert
        violations.ShouldBeEmpty("Classes expose state through properties, not public instance fields");
    }

    [Fact]
    public void PublicTypesAndMembers_ShouldHaveXmlDocumentation()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<MemberDeclarationSyntax>()
                                                                      .Where(member => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or MethodDeclarationSyntax
                                                                                                  or PropertyDeclarationSyntax or ConstructorDeclarationSyntax or EventDeclarationSyntax
                                                                                                  or EnumMemberDeclarationSyntax)
                                                                      .Where(IsPubliclyVisible)
                                                                      .Where(member => !member.Modifiers.Any(SyntaxKind.PartialKeyword))
                                                                      .Where(member => !HasDocumentation(member) && !IsDocumentationWarningSuppressed(member))
                                                                      .Select(member => $"{file.Location(member)}: {MemberName(member)}"));

        // Assert
        violations.ShouldBeEmpty("Public types and members must have XML documentation (/// <summary> or /// <inheritdoc/>)");
    }

    [Fact]
    public void AsyncMethods_ShouldEndWithAsync()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<MethodDeclarationSyntax>()
                                                                      .Where(method => IsTaskType(method.ReturnType) && !method.Identifier.Text.EndsWith("Async"))
                                                                      .Where(method => !method.Modifiers.Any(SyntaxKind.OverrideKeyword) && method.ExplicitInterfaceSpecifier == null)
                                                                      .Where(method => !_asyncSuffixExemptMethods.Contains(method.Identifier.Text))
                                                                      .Select(method => $"{file.Location(method)}: {method.Identifier.Text}"));

        // Assert
        violations.ShouldBeEmpty("Methods returning Task/ValueTask must end with Async");
    }

    private static bool IsOnSameLine(SyntaxToken token, StatementSyntax statement)
        => statement is not BlockSyntax
           && token.GetLocation().GetLineSpan().EndLinePosition.Line == statement.GetLocation().GetLineSpan().StartLinePosition.Line;

    private static string ExpectedNamespace(SourceFile file)
        => string.IsNullOrEmpty(file.Folder) ? file.Project : $"{file.Project}.{file.Folder.Replace('/', '.')}";

    private static IEnumerable<MemberDeclarationSyntax> TopLevelTypes(SourceFile file)
        => file.Root.Members
                    .SelectMany(m => m is BaseNamespaceDeclarationSyntax ns ? ns.Members : [m])
                    .Where(m => m is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax);

    private static string TypeName(MemberDeclarationSyntax member) => member switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.Text,
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.Text,
        _ => string.Empty,
    };

    private static string MemberName(MemberDeclarationSyntax member) => member switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.Text,
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.Text,
        MethodDeclarationSyntax method => method.Identifier.Text,
        PropertyDeclarationSyntax property => property.Identifier.Text,
        ConstructorDeclarationSyntax constructor => constructor.Identifier.Text + " constructor",
        EventDeclarationSyntax @event => @event.Identifier.Text,
        EnumMemberDeclarationSyntax enumMember => enumMember.Identifier.Text,
        _ => member.Kind().ToString(),
    };

    private static bool IsPascalCase(string name) => name.Length > 0 && char.IsUpper(name[0]) && !name.Contains('_');

    private static bool IsUnderscoreCamelCase(string name) => name.Length > 1 && name[0] == '_' && char.IsLower(name[1]) && !name[1..].Contains('_');

    private static bool IsPrivateOrInternal(SyntaxTokenList modifiers)
        => !modifiers.Any(SyntaxKind.PublicKeyword) && !(modifiers.Any(SyntaxKind.ProtectedKeyword) && !modifiers.Any(SyntaxKind.PrivateKeyword));

    private static bool IsTaskType(TypeSyntax type)
    {
        var name = type switch
        {
            GenericNameSyntax generic => generic.Identifier.Text,
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            _ => string.Empty,
        };

        return name is "Task" or "ValueTask";
    }

    /// <summary>
    /// A member is publicly visible when it and all of its containing types are public (or it is an enum member / interface member of such a type).
    /// </summary>
    private static bool IsPubliclyVisible(MemberDeclarationSyntax member)
    {
        for (SyntaxNode node = member; node is MemberDeclarationSyntax current and not BaseNamespaceDeclarationSyntax; node = node.Parent)
        {
            var isPublic = current.Modifiers.Any(SyntaxKind.PublicKeyword)
                           || current is EnumMemberDeclarationSyntax
                           || (current.Parent is InterfaceDeclarationSyntax && !current.Modifiers.Any(SyntaxKind.PrivateKeyword));

            if (!isPublic)
                return false;
        }

        return true;
    }

    /// <summary>
    /// True when the member is inside a <c>#pragma warning disable CS1591</c> region, i.e. the missing documentation is intentional.
    /// </summary>
    private static bool IsDocumentationWarningSuppressed(MemberDeclarationSyntax member)
    {
        var lastPragma = member.SyntaxTree
                               .GetRoot()
                               .DescendantTrivia()
                               .Where(trivia => trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia) && trivia.SpanStart < member.SpanStart)
                               .Select(trivia => (PragmaWarningDirectiveTriviaSyntax)trivia.GetStructure())
                               .LastOrDefault(pragma => pragma.ErrorCodes.Any(code => code.ToString() == _missingXmlCommentWarning));

        return lastPragma != null && lastPragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword);
    }

    private static bool HasDocumentation(MemberDeclarationSyntax member)
        => member.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
}
