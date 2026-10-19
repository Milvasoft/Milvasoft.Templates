using Milvonion.Api;
using Milvonion.Application;
using Milvonion.Domain;
using Milvonion.Infrastructure;
using Milvonion.UnitTests.TestSupport;
using NetArchTest.Rules;
using System.Reflection;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Clean Architecture layer rules. Dependencies may only point inwards: Api -> Infrastructure -> Application -> Domain.
/// </summary>
[Trait("Architecture Tests", "Layer dependency rules.")]
public class LayerDependencyTests
{
    private const string _domainNamespace = "Milvonion.Domain";
    private const string _applicationNamespace = "Milvonion.Application";
    private const string _infrastructureNamespace = "Milvonion.Infrastructure";
    private const string _apiNamespace = "Milvonion.Api";

    public static TheoryData<string, string> ForbiddenLayerDependencies => new()
    {
        { _domainNamespace, _applicationNamespace },
        { _domainNamespace, _infrastructureNamespace },
        { _domainNamespace, _apiNamespace },
        { _applicationNamespace, _infrastructureNamespace },
        { _applicationNamespace, _apiNamespace },
        { _infrastructureNamespace, _apiNamespace },
    };

    public static TheoryData<string, string> ForbiddenLibraryDependencies => new()
    {
        // Domain holds entities and enums only, use cases and request pipeline belong to the application layer.
        { _domainNamespace, "MediatR" },
        { _domainNamespace, "FluentValidation" },
        { _domainNamespace, "Mapster" },
        { _domainNamespace, "Microsoft.AspNetCore.Mvc" },
        { _domainNamespace, "Npgsql" },

        // Application talks to external systems only through the abstractions under Milvonion.Application.Interfaces.
        { _applicationNamespace, "Microsoft.AspNetCore.Mvc" },
        { _applicationNamespace, "Npgsql" },
        { _applicationNamespace, "ClosedXML" },
        { _applicationNamespace, "Serilog" },
        { _applicationNamespace, "Swashbuckle" },
    };

    /// <summary>
    /// Deliberate, reviewed exceptions to <see cref="ForbiddenLibraryDependencies"/>. Key: "layer|library", value: allowed type full names or namespace prefixes.
    /// </summary>
    private static readonly Dictionary<string, string[]> _allowedLibraryDependencies = new()
    {
        // Authorization filter attributes are shared by every controller and belong to the template's application layer.
        [$"{_applicationNamespace}|Microsoft.AspNetCore.Mvc"] = ["Milvonion.Application.Utils.Attributes."],

        // Maps PostgreSQL unique violations (23505) to localized duplicate messages.
        [$"{_applicationNamespace}|Npgsql"] = ["Milvonion.Application.Utils.Helpers.UserUniqueConstraintHelper"],
    };

    [Theory]
    [MemberData(nameof(ForbiddenLayerDependencies))]
    public void Layer_ShouldNotDependOnOuterLayer(string layer, string forbiddenLayer)
    {
        // Arrange
        var assembly = GetAssembly(layer);

        // Act
        var result = Types.InAssembly(assembly)
                          .ShouldNot()
                          .HaveDependencyOn(forbiddenLayer)
                          .GetResult();

        // Assert
        (result.FailingTypeNames ?? []).ShouldBeEmpty($"{layer} must not depend on {forbiddenLayer}");
    }

    [Theory]
    [MemberData(nameof(ForbiddenLayerDependencies))]
    public void LayerAssembly_ShouldNotReferenceOuterLayerAssembly(string layer, string forbiddenLayer)
    {
        // Arrange
        var assembly = GetAssembly(layer);

        // Act
        var references = assembly.GetReferencedAssemblies().Select(a => a.Name).Where(name => name == forbiddenLayer);

        // Assert
        references.ShouldBeEmpty($"{layer} project must not reference {forbiddenLayer}");
    }

    [Theory]
    [MemberData(nameof(ForbiddenLibraryDependencies))]
    public void Layer_ShouldNotDependOnForbiddenLibrary(string layer, string forbiddenLibrary)
    {
        // Arrange
        var assembly = GetAssembly(layer);
        var allowed = _allowedLibraryDependencies.GetValueOrDefault($"{layer}|{forbiddenLibrary}", []);

        // Act
        var result = Types.InAssembly(assembly)
                          .ShouldNot()
                          .HaveDependencyOn(forbiddenLibrary)
                          .GetResult();

        var violations = (result.FailingTypeNames ?? []).Where(name => !allowed.Any(a => name == a || (a.EndsWith('.') && name.StartsWith(a))));

        // Assert
        violations.ShouldBeEmpty($"{layer} must not depend on {forbiddenLibrary}");
    }

    [Fact]
    public void Controllers_ShouldOnlyLiveInApiLayer()
    {
        // Arrange
        var assemblies = new[] { DomainAssembly.Assembly, ApplicationAssembly.Assembly, InfrastructureAssembly.Assembly };

        // Act
        var result = Types.InAssemblies(assemblies)
                          .ShouldNot()
                          .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
                          .GetResult();

        // Assert
        (result.FailingTypeNames ?? []).ShouldBeEmpty("Controllers must be declared in Milvonion.Api");
    }

    private static Assembly GetAssembly(string layer) => layer switch
    {
        _domainNamespace => DomainAssembly.Assembly,
        _applicationNamespace => ApplicationAssembly.Assembly,
        _infrastructureNamespace => InfrastructureAssembly.Assembly,
        _apiNamespace => PresentationAssembly.Assembly,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
    };
}
