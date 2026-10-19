using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Milvonion.UnitTests.TestSupport;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Dependency usage rules: primary constructor + <c>private readonly _x = X;</c> field pattern and
/// configuration access only through options classes.
/// </summary>
[Trait("Architecture Tests", "Dependency usage rules.")]
public class DependencyConventionTests
{
    private const string _configurationType = "IConfiguration";

    /// <summary>
    /// Composition root files, the only place where <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> may be read freely.
    /// </summary>
    private static readonly string[] _compositionRootSuffixes = ["ServiceCollectionExtensions.cs", "ApplicationBuilderExtensions.cs", "Program.cs"];

    /// <summary>
    /// Configuration members that are allowed outside the composition root (connection strings are not options).
    /// </summary>
    private static readonly HashSet<string> _allowedConfigurationMembers = ["GetConnectionString"];

    [Fact]
    public void Configuration_ShouldOnlyBeReadInCompositionRoot()
    {
        // Act
        var violations = SourceCode.Files.Where(file => !_compositionRootSuffixes.Any(file.RelativePath.EndsWith))
                                         .SelectMany(file => ConfigurationReads(file).Select(node => $"{file.Location(node)}: {node}"));

        // Assert
        violations.ShouldBeEmpty("Configuration values must be bound to an options class in the composition root and injected, services must not read IConfiguration");
    }

    [Fact]
    public void PrimaryConstructorParameters_ShouldOnlyBeUsedInFieldInitializers()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<TypeDeclarationSyntax>()
                                                                      .Where(type => type.ParameterList != null && type is ClassDeclarationSyntax or RecordDeclarationSyntax)
                                                                      .SelectMany(type => PrimaryConstructorParameterUsagesInMembers(type)
                                                                      .Select(usage => $"{file.Location(usage)}: {type.Identifier.Text} uses primary constructor parameter '{usage.Identifier.Text}' directly, assign it to a private readonly field")));

        // Assert
        violations.ShouldBeEmpty("Primary constructor parameters must be captured as 'private readonly _x = X;' fields");
    }

    [Fact]
    public void DependencyFields_ShouldBePrivateReadonly()
    {
        // Act
        var violations = SourceCode.Files.SelectMany(file => file.Root.DescendantNodes()
                                                                      .OfType<TypeDeclarationSyntax>()
                                                                      .Where(type => type.ParameterList != null)
                                                                      .SelectMany(type => type.Members
                                                                                              .OfType<FieldDeclarationSyntax>()
                                                                                              .Where(field => IsInitializedFromParameter(field, type))
                                                                                              .Where(field => !field.Modifiers.Any(SyntaxKind.PrivateKeyword) || !field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
                                                                                              .Select(field => $"{file.Location(field)}: {type.Identifier.Text}.{field.Declaration.Variables[0].Identifier.Text}")));

        // Assert
        violations.ShouldBeEmpty("Fields that capture primary constructor parameters must be 'private readonly'");
    }

    /// <summary>
    /// Finds member accesses and element accesses on variables, fields and parameters typed as IConfiguration
    /// (and <c>GetService&lt;IConfiguration&gt;()</c> results) except <see cref="_allowedConfigurationMembers"/>.
    /// </summary>
    private static IEnumerable<SyntaxNode> ConfigurationReads(SourceFile file)
    {
        var names = new HashSet<string>();

        foreach (var node in file.Root.DescendantNodes())
        {
            switch (node)
            {
                case ParameterSyntax { Type: IdentifierNameSyntax { Identifier.Text: _configurationType } } parameter:
                    names.Add(parameter.Identifier.Text);
                    break;
                case VariableDeclarationSyntax { Type: IdentifierNameSyntax { Identifier.Text: _configurationType } } declaration:
                    names.UnionWith(declaration.Variables.Select(v => v.Identifier.Text));
                    break;
                case VariableDeclaratorSyntax { Initializer.Value: var value } declarator when value.DescendantNodesAndSelf().OfType<GenericNameSyntax>().Any(IsConfigurationServiceLookup):
                    names.Add(declarator.Identifier.Text);
                    break;
            }
        }

        if (names.Count == 0)
            return [];

        var memberReads = file.Root.DescendantNodes()
                                   .OfType<MemberAccessExpressionSyntax>()
                                   .Where(access => access.Expression is IdentifierNameSyntax identifier && names.Contains(identifier.Identifier.Text))
                                   .Where(access => !_allowedConfigurationMembers.Contains(access.Name.Identifier.Text))
                                   .Cast<SyntaxNode>();

        var indexerReads = file.Root.DescendantNodes()
                                    .OfType<ElementAccessExpressionSyntax>()
                                    .Where(access => access.Expression is IdentifierNameSyntax identifier && names.Contains(identifier.Identifier.Text));

        return memberReads.Concat(indexerReads);
    }

    private static bool IsConfigurationServiceLookup(GenericNameSyntax name)
        => name.Identifier.Text is "GetService" or "GetRequiredService"
           && name.TypeArgumentList.Arguments.Any(a => a is IdentifierNameSyntax { Identifier.Text: _configurationType });

    /// <summary>
    /// Returns identifiers inside methods, accessors, constructors, operators and expression bodied members
    /// that refer to a primary constructor parameter (and are not shadowed by a local declaration of that member).
    /// </summary>
    private static IEnumerable<IdentifierNameSyntax> PrimaryConstructorParameterUsagesInMembers(TypeDeclarationSyntax type)
    {
        var parameters = type.ParameterList.Parameters.Select(p => p.Identifier.Text).ToHashSet();

        foreach (var member in type.Members.Where(m => m is BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax))
        {
            if (member is PropertyDeclarationSyntax { AccessorList: null, ExpressionBody: null })
                continue;

            var body = member is PropertyDeclarationSyntax property && property.Initializer != null && property.AccessorList != null
                ? (SyntaxNode)property.AccessorList
                : member;

            var locals = body.DescendantNodes()
                             .Select(DeclaredName)
                             .Where(name => name != null)
                             .ToHashSet();

            foreach (var identifier in body.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                var name = identifier.Identifier.Text;

                if (!parameters.Contains(name) || locals.Contains(name) || IsMemberName(identifier))
                    continue;

                yield return identifier;
            }
        }
    }

    private static string DeclaredName(SyntaxNode node) => node switch
    {
        VariableDeclaratorSyntax variable => variable.Identifier.Text,
        ParameterSyntax parameter => parameter.Identifier.Text,
        SingleVariableDesignationSyntax designation => designation.Identifier.Text,
        ForEachStatementSyntax forEach => forEach.Identifier.Text,
        CatchDeclarationSyntax @catch => @catch.Identifier.Text,
        LocalFunctionStatementSyntax localFunction => localFunction.Identifier.Text,
        _ => null,
    };

    /// <summary>
    /// True for <c>x.Name</c> (right side of a member access), named arguments and object initializer targets, which are not parameter references.
    /// </summary>
    private static bool IsMemberName(IdentifierNameSyntax identifier) => identifier.Parent switch
    {
        MemberAccessExpressionSyntax memberAccess => memberAccess.Name == identifier,
        MemberBindingExpressionSyntax => true,
        NameColonSyntax => true,
        AssignmentExpressionSyntax assignment => assignment.Left == identifier && assignment.Parent is InitializerExpressionSyntax,
        _ => false,
    };

    private static bool IsInitializedFromParameter(FieldDeclarationSyntax field, TypeDeclarationSyntax type)
    {
        var parameters = type.ParameterList.Parameters.Select(p => p.Identifier.Text).ToHashSet();

        return field.Declaration.Variables.Any(v => v.Initializer?.Value is IdentifierNameSyntax identifier && parameters.Contains(identifier.Identifier.Text));
    }
}
