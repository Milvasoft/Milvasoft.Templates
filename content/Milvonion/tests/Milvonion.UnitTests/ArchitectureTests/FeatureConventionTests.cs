using FluentValidation;
using MediatR;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.CQRS.Query;
using Milvonion.Application;
using Milvonion.UnitTests.TestSupport;
using System.Reflection;

namespace Milvonion.UnitTests.ArchitectureTests;

/// <summary>
/// CQRS conventions of the application layer. Every use case lives in its own folder
/// (Features/{Area}/{Feature}) and consists of a request, its handler and optionally its validator.
/// </summary>
[Trait("Architecture Tests", "CQRS feature conventions.")]
public class FeatureConventionTests
{
    private const string _featuresNamespace = "Milvonion.Application.Features";
    private const string _commandSuffix = "Command";
    private const string _querySuffix = "Query";
    private const string _handlerSuffix = "Handler";
    private const string _validatorSuffix = "Validator";

    private static readonly Type[] _concreteTypes = [.. ApplicationAssembly.Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !IsCompilerGenerated(t))];

    private static readonly Type[] _requests = [.. _concreteTypes.Where(t => t.GetInterfaces().Any(i => i == typeof(IBaseRequest)))];

    private static readonly Type[] _handlers = [.. _concreteTypes.Where(t => GetHandledRequest(t) != null)];

    private static readonly Type[] _validators = [.. _concreteTypes.Where(t => GetValidatedType(t) != null)];

    [Fact]
    public void Requests_ShouldLiveInFeatureFolder()
    {
        // Act
        var violations = _requests.Where(t => !IsFeatureNamespace(t.Namespace))
                                  .Select(t => $"{t.FullName}: requests must be declared under {_featuresNamespace}.{{Area}}.{{Feature}}");

        // Assert
        violations.ShouldBeEmpty("MediatR requests must live in a feature folder");
    }

    [Fact]
    public void Requests_ShouldBeNamedAfterTheirFeatureFolder()
    {
        // Act
        var violations = _requests.Where(t => IsFeatureNamespace(t.Namespace))
                                  .Where(t => t.Name != FeatureName(t) + _commandSuffix && t.Name != FeatureName(t) + _querySuffix)
                                  .Select(t => $"{t.FullName}: expected {FeatureName(t)}{_commandSuffix} or {FeatureName(t)}{_querySuffix}");

        // Assert
        violations.ShouldBeEmpty("A request must be named {Feature}Command or {Feature}Query");
    }

    [Fact]
    public void Commands_ShouldImplementCommandInterface()
    {
        // Act
        var violations = _requests.Where(t => t.Name.EndsWith(_commandSuffix))
                                  .Where(t => !Implements(t, typeof(ICommand), typeof(ICommand<>)))
                                  .Select(t => $"{t.FullName}: must implement ICommand or ICommand<T>");

        // Assert
        violations.ShouldBeEmpty("Commands must implement the Milvasoft command abstractions");
    }

    [Fact]
    public void Queries_ShouldImplementQueryInterface()
    {
        // Act
        var violations = _requests.Where(t => t.Name.EndsWith(_querySuffix))
                                  .Where(t => !Implements(t, typeof(IQuery), typeof(IQuery<>), typeof(IListRequestQuery<>)))
                                  .Select(t => $"{t.FullName}: must implement IQuery, IQuery<T> or IListRequestQuery<T>");

        // Assert
        violations.ShouldBeEmpty("Queries must implement the Milvasoft query abstractions");
    }

    [Fact]
    public void CommandAndQueryAbstractions_ShouldMatchTypeNameSuffix()
    {
        // Act
        var commandsNamedAsQuery = _requests.Where(t => Implements(t, typeof(ICommand), typeof(ICommand<>)) && !t.Name.EndsWith(_commandSuffix))
                                            .Select(t => $"{t.FullName}: implements ICommand but is not named *{_commandSuffix}");

        var queriesNamedAsCommand = _requests.Where(t => Implements(t, typeof(IQuery), typeof(IQuery<>), typeof(IListRequestQuery<>)) && !t.Name.EndsWith(_querySuffix))
                                             .Select(t => $"{t.FullName}: implements IQuery but is not named *{_querySuffix}");

        // Assert
        commandsNamedAsQuery.Concat(queriesNamedAsCommand).ShouldBeEmpty("Command/query abstractions and type name suffixes must agree");
    }

    [Fact]
    public void Requests_ShouldHaveExactlyOneHandlerInSameFeatureFolder()
    {
        // Act
        var violations = new List<string>();

        foreach (var request in _requests)
        {
            var handlers = _handlers.Where(h => GetHandledRequest(h) == request).ToList();

            if (handlers.Count != 1)
            {
                violations.Add($"{request.FullName}: has {handlers.Count} handler(s)");
                continue;
            }

            var handler = handlers[0];

            if (handler.Name != request.Name + _handlerSuffix)
                violations.Add($"{handler.FullName}: expected name {request.Name}{_handlerSuffix}");

            if (handler.Namespace != request.Namespace)
                violations.Add($"{handler.FullName}: must be in the same namespace as {request.FullName}");
        }

        // Assert
        violations.ShouldBeEmpty("Every request must have exactly one {Request}Handler next to it");
    }

    [Fact]
    public void Handlers_ShouldBeNamedWithHandlerSuffix()
    {
        // Act
        var violations = _handlers.Where(t => !t.Name.EndsWith(_handlerSuffix))
                                  .Select(t => $"{t.FullName}: must end with {_handlerSuffix}");

        // Assert
        violations.ShouldBeEmpty("Request handlers must be named *Handler");
    }

    [Fact]
    public void Handlers_ShouldBePublicAndNotDependOnMediator()
    {
        // Act
        var notPublic = _handlers.Where(t => !t.IsPublic)
                                 .Select(t => $"{t.FullName}: must be public");

        var usesMediator = _handlers.Where(t => ConstructorParameterTypes(t).Any(p => p == typeof(IMediator) || p == typeof(ISender) || p == typeof(IPublisher)))
                                    .Select(t => $"{t.FullName}: handlers must not send other requests through the mediator, move shared logic into a helper/extension");

        // Assert
        notPublic.Concat(usesMediator).ShouldBeEmpty("Handler shape rules");
    }

    [Fact]
    public void Handlers_ShouldNotDependOnOtherHandlers()
    {
        // Act
        var violations = _handlers.SelectMany(t => ConstructorParameterTypes(t).Where(p => GetHandledRequest(p) != null)
                                                                               .Select(p => $"{t.FullName}: depends on handler {p.FullName}"));

        // Assert
        violations.ShouldBeEmpty("Handlers must not call each other, move shared logic into a helper/extension");
    }

    [Fact]
    public void Validators_ShouldBeSealedAndNamedAfterRequestInSameFeatureFolder()
    {
        // Act
        var violations = new List<string>();

        foreach (var validator in _validators)
        {
            var validated = GetValidatedType(validator);

            if (!validator.IsSealed)
                violations.Add($"{validator.FullName}: must be sealed");

            if (!validator.Name.EndsWith(_validatorSuffix))
                violations.Add($"{validator.FullName}: must end with {_validatorSuffix}");

            if (!_requests.Contains(validated))
                continue;

            if (validator.Name != validated.Name + _validatorSuffix)
                violations.Add($"{validator.FullName}: expected name {validated.Name}{_validatorSuffix}");

            if (validator.Namespace != validated.Namespace)
                violations.Add($"{validator.FullName}: must be in the same namespace as {validated.FullName}");
        }

        // Assert
        violations.ShouldBeEmpty("Request validators must be sealed {Request}Validator classes next to the request");
    }

    [Fact]
    public void Requests_ShouldHaveAtMostOneValidator()
    {
        // Act
        var violations = _validators.GroupBy(GetValidatedType)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => $"{g.Key.FullName}: has {g.Count()} validators ({string.Join(", ", g.Select(v => v.Name))})");

        // Assert
        violations.ShouldBeEmpty("A request must have a single validator");
    }

    [Fact]
    public void FeatureFolders_ShouldOnlyContainTheFeatureTypes()
    {
        // Act
        var violations = _concreteTypes.Where(t => IsFeatureNamespace(t.Namespace) && !t.IsNested)
                                       .Where(t => !_requests.Contains(t) && !_handlers.Contains(t) && !_validators.Contains(t))
                                       .Where(t => !IsAllowedFeatureHelper(t))
                                       .Select(t => $"{t.FullName}: move DTOs to Dtos/ and shared logic to Utils/");

        // Assert
        violations.ShouldBeEmpty("Feature folders contain only the request, its handler and its validator");
    }

    /// <summary>
    /// Static helper classes that are private to one feature (e.g. Account/Login/LockoutMessages) are allowed.
    /// </summary>
    private static bool IsAllowedFeatureHelper(Type type) => type.IsAbstract && type.IsSealed;

    /// <summary>
    /// True for "{Features}.{Area}.{Feature}" and for an area that groups its features one level deeper
    /// ("{Features}.{Area}.{Group}.{Feature}", e.g. ContentManagement/Contents/CreateContent). Counted after the features
    /// namespace, so a multi segment project name (e.g. Acme.Shop) works too.
    /// </summary>
    private static bool IsFeatureNamespace(string ns) => ns != null && ns.StartsWith(_featuresNamespace + ".") && ns[(_featuresNamespace.Length + 1)..].Split('.').Length is 2 or 3;

    private static string FeatureName(Type type) => type.Namespace.Split('.')[^1];

    private static bool Implements(Type type, params Type[] interfaces)
        => type.GetInterfaces().Any(i => interfaces.Contains(i) || (i.IsGenericType && interfaces.Contains(i.GetGenericTypeDefinition())));

    private static Type GetHandledRequest(Type type)
        => type.GetInterfaces()
               .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) || i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
               .Select(i => i.GetGenericArguments()[0])
               .FirstOrDefault();

    private static Type GetValidatedType(Type type)
    {
        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                return current.GetGenericArguments()[0];
        }

        return null;
    }

    private static IEnumerable<Type> ConstructorParameterTypes(Type type)
        => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType);

    private static bool IsCompilerGenerated(Type type)
        => type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false) || type.Name.Contains('<');
}
