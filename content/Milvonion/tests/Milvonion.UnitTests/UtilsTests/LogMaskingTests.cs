using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Types.Structs;
using Milvonion.Application;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Features.Users.UpdateUser;
using Milvonion.Infrastructure.Logging;
using Moq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Milvonion.UnitTests.UtilsTests;

/// <summary>
/// The <c>[Log]</c> interceptor writes the arguments and the result of a method when <c>LogDefaultParameters</c> is on.
/// Only string properties marked with <see cref="LogMaskedAttribute"/> are masked, so secrets in requests must carry it
/// and a logged handler must not carry a secret that cannot be masked (an <c>UpdateProperty&lt;string&gt;</c>, a token object).
/// </summary>
[Trait("Utils Unit Tests", "Method log masking.")]
public partial class LogMaskingTests
{
    [GeneratedRegex("Password|Token|Secret")]
    private static partial Regex SensitiveName();

    [Fact]
    public void SensitiveStringPropertiesOfRequests_ShouldBeLogMasked()
    {
        // Arrange
        var requestTypes = ApplicationAssembly.Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IBaseRequest).IsAssignableFrom(t));

        // Act
        var violations = requestTypes.SelectMany(t => t.GetProperties())
                                     .Where(p => p.PropertyType == typeof(string) && SensitiveName().IsMatch(p.Name))
                                     .Where(p => p.GetCustomAttribute<LogMaskedAttribute>() == null)
                                     .Select(p => $"{p.DeclaringType.Name}.{p.Name}");

        // Assert
        violations.Should().BeEmpty("a password or token in a request must be marked [LogMasked]");
    }

    [Fact]
    public void LoggedHandlers_ShouldNotCarryUnmaskableSecrets()
    {
        // Arrange
        var loggedHandlers = ApplicationAssembly.Assembly.GetTypes().Where(t => t.GetCustomAttribute<LogAttribute>() != null);

        // Act: the request and the response data of every handler marked [Log].
        var violations = loggedHandlers.SelectMany(handler => handler.GetInterfaces()
                                                                      .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                                                                      .SelectMany(i => new[] { i.GetGenericArguments()[0], ResponseDataType(i.GetGenericArguments()[1]) })
                                                                      .Where(t => t != null)
                                                                      .SelectMany(t => t.GetProperties())
                                                                      .Where(p => p.PropertyType != typeof(string) && SensitiveName().IsMatch(p.Name))
                                                                      .Select(p => $"{handler.Name}: {p.DeclaringType.Name}.{p.Name} ({p.PropertyType.Name})"));

        // Assert
        violations.Should().BeEmpty("[LogMasked] masks string properties only; remove [Log] from a handler that carries any other kind of secret");
    }

    [Fact]
    public async Task LogInterceptor_WithDefaultParameters_ShouldMaskOnlyTheMarkedStringProperties()
    {
        // Arrange
        var entries = new List<string>();
        var probe = BuildProbe(logDefaultParameters: true, entries);

        // Act
        await probe.RunAsync(new LoginCommand { UserName = "probe-user", Password = "login-secret", DeviceId = "device" });
        await probe.RunAsync(new UpdateUserCommand { Id = 1, NewPassword = new UpdateProperty<string>("update-secret") });

        // Assert: the marked string is masked; an UpdateProperty<string> is not, which is why UpdateUser carries no [Log].
        entries.Should().HaveCount(2);
        entries[0].Should().Contain("probe-user").And.NotContain("login-secret");
        entries[1].Should().Contain("update-secret");
    }

    [Fact]
    public async Task LogInterceptor_WithoutDefaultParameters_ShouldNotWriteArguments()
    {
        // Arrange
        var entries = new List<string>();
        var probe = BuildProbe(logDefaultParameters: false, entries);

        // Act
        await probe.RunAsync(new UpdateUserCommand { Id = 1, NewPassword = new UpdateProperty<string>("update-secret") });

        // Assert
        entries.Should().ContainSingle().Which.Should().NotContain("update-secret").And.NotContain("MethodName");
    }

    [Fact]
    public void MilvonionDbLogger_EntryWithoutMethod_ShouldWriteNothing()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var dbLogger = new MilvonionDbLogger(loggerFactory.Object);

        // Act: what the interceptor sends with LogDefaultParameters off.
        dbLogger.Log("{}");

        // Assert
        logger.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
    }

    private static Type ResponseDataType(Type responseType) => responseType.IsGenericType ? responseType.GetGenericArguments()[0] : null;

    private static ILoggedProbe BuildProbe(bool logDefaultParameters, List<string> entries)
    {
        var logger = new Mock<IMilvaLogger>();
        logger.Setup(l => l.Log(It.IsAny<string>())).Callback<string>(entries.Add);
        logger.Setup(l => l.LogAsync(It.IsAny<string>())).Callback<string>(entries.Add).Returns(Task.CompletedTask);

        var services = new ServiceCollection();

        services.AddSingleton(logger.Object);
        services.AddScoped<ILoggedProbe, LoggedProbe>();

        services.AddMilvaInterception([typeof(ILoggedProbe)])
                .WithLogInterceptor(opt =>
                {
                    opt.LogDefaultParameters = logDefaultParameters;
                    opt.AsyncLogging = false;
                });

        return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ILoggedProbe>();
    }

    public interface ILoggedProbe : IInterceptable
    {
        Task<Response> RunAsync(object request);
    }

    public class LoggedProbe : ILoggedProbe
    {
        [Log]
        public Task<Response> RunAsync(object request) => Task.FromResult(Response.Success());
    }
}
