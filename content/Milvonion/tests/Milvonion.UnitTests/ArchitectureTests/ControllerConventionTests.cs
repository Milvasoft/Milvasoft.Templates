using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Milvonion.Api;
using Milvonion.Application.Interfaces;
using Milvonion.UnitTests.TestSupport;
using System.Reflection;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// Controller conventions. Controllers are thin: they route the request to MediatR (or an application service)
/// and never touch persistence or raw configuration.
/// </summary>
[Trait("Architecture Tests", "Controller conventions.")]
public class ControllerConventionTests
{
    private const string _controllersNamespace = "Milvonion.Api.Controllers";
    private const string _controllerSuffix = "Controller";
    private const string _asyncSuffix = "Async";

    private static readonly Type[] _controllers = [.. PresentationAssembly.Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))];

    [Fact]
    public void Controllers_ShouldFollowNamingAndPlacement()
    {
        // Act
        var badName = _controllers.Where(t => !t.Name.EndsWith(_controllerSuffix))
                                  .Select(t => $"{t.FullName}: must end with {_controllerSuffix}");

        var badNamespace = _controllers.Where(t => t.Namespace != _controllersNamespace)
                                       .Select(t => $"{t.FullName}: must be in {_controllersNamespace}");

        // Assert
        badName.Concat(badNamespace).ShouldBeEmpty("Controllers must be named *Controller and live in Milvonion.Api/Controllers");
    }

    [Fact]
    public void Controllers_ShouldHaveApiControllerAndRouteAttributes()
    {
        // Act
        var missingApiController = _controllers.Where(t => t.GetCustomAttribute<ApiControllerAttribute>() == null)
                                               .Select(t => $"{t.FullName}: missing [ApiController]");

        var missingRoute = _controllers.Where(t => t.GetCustomAttribute<RouteAttribute>() == null)
                                       .Select(t => $"{t.FullName}: missing [Route]");

        // Assert
        missingApiController.Concat(missingRoute).ShouldBeEmpty("Controllers must declare [ApiController] and [Route]");
    }

    [Fact]
    public void Controllers_ShouldNotDependOnPersistenceOrConfiguration()
    {
        // Act
        var violations = _controllers.SelectMany(t => t.GetConstructors()
                                                       .SelectMany(c => c.GetParameters())
                                                       .Where(p => IsForbiddenControllerDependency(p.ParameterType))
                                                       .Select(p => $"{t.FullName}: injects {p.ParameterType.Name}"));

        // Assert
        violations.ShouldBeEmpty("Controllers must not inject repositories, DbContext or IConfiguration (bind settings to an options class)");
    }

    [Fact]
    public void Actions_ShouldHaveHttpMethodAttribute()
    {
        // Act
        var violations = Actions().Where(m => !m.GetCustomAttributes<HttpMethodAttribute>().Any())
                                  .Select(m => $"{m.DeclaringType.FullName}.{m.Name}: missing [Http*] attribute");

        // Assert
        violations.ShouldBeEmpty("Every action must declare its HTTP method explicitly");
    }

    [Fact]
    public void AsyncActions_ShouldEndWithAsync()
    {
        // Act
        var violations = Actions().Where(m => typeof(Task).IsAssignableFrom(m.ReturnType) || (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
                                  .Where(m => !m.Name.EndsWith(_asyncSuffix))
                                  .Select(m => $"{m.DeclaringType.FullName}.{m.Name}");

        // Assert
        violations.ShouldBeEmpty("Async actions must end with Async");
    }

    [Fact]
    public void MediatorActions_ShouldAcceptCancellationToken()
    {
        // Act
        var violations = _controllers.Where(t => t.GetConstructors().SelectMany(c => c.GetParameters()).Any(p => p.ParameterType == typeof(MediatR.IMediator)))
                                     .SelectMany(t => Actions().Where(m => m.DeclaringType == t))
                                     .Where(m => typeof(Task).IsAssignableFrom(m.ReturnType))
                                     .Where(m => !m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)))
                                     .Select(m => $"{m.DeclaringType.FullName}.{m.Name}");

        // Assert
        violations.ShouldBeEmpty("Async actions of MediatR controllers must accept a CancellationToken and pass it to the mediator");
    }

    private static IEnumerable<MethodInfo> Actions()
        => _controllers.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                       .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() == null);

    private static bool IsForbiddenControllerDependency(Type type)
        => typeof(IConfiguration).IsAssignableFrom(type)
           || typeof(DbContext).IsAssignableFrom(type)
           || type == typeof(IMilvonionDbContextAccessor)
           || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IMilvonionRepositoryBase<>));
}
