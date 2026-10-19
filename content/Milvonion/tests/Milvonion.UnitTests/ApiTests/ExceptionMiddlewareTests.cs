using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvonion.Api.Middlewares;
using Milvonion.Application.Utils.Constants;
using Milvonion.UnitTests.TestSupport;
using Npgsql;
using System.Text.Json;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="ExceptionMiddleware"/>: every exception escaping the pipeline becomes a JSON envelope with the
/// right status code, and nothing sensitive reaches the log.
/// </summary>
[Trait("Api Unit Tests", "Exception middleware.")]
public class ExceptionMiddlewareTests
{
    /// <summary>Captures every log entry written through the factory handed to the middleware.</summary>
    private sealed class CapturingLoggerProvider : ILoggerProvider, ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => this;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception)));

        public void Dispose()
        {
        }
    }

    private static DefaultHttpContext CreateContext(int? presetStatus = null)
    {
        var services = new ServiceCollection();

        services.AddSingleton(FakeLocalizer.Create().Object);
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        context.Request.Method = "POST";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("api.test");
        context.Request.Path = "/api/v1.0/Users";
        context.Response.Body = new MemoryStream();

        if (presetStatus.HasValue)
            context.Response.StatusCode = presetStatus.Value;

        return context;
    }

    private static async Task<JsonElement> ReadEnvelopeAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;

        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        return document.RootElement.Clone();
    }

    private static string FirstMessage(JsonElement envelope) => envelope.GetProperty("messages")[0].GetProperty("message").GetString();

    private static ExceptionMiddleware Middleware(Exception toThrow, ILoggerFactory loggerFactory = null)
        => new(_ => throw toThrow, loggerFactory ?? NullLoggerFactory.Instance);

    private static PostgresException Postgres(string sqlState, string message = "db error") => new(message, "ERROR", "ERROR", sqlState);

    [Fact]
    public async Task Invoke_ShouldPassThrough_WhenNextDoesNotThrow()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        var middleware = new ExceptionMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, NullLoggerFactory.Instance);

        // Act
        await middleware.Invoke(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task Invoke_ShouldWrite403Envelope_ForForbiddenUserFriendlyException()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(new MilvaUserFriendlyException(MessageKey.Forbidden)).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
        envelope.GetProperty("statusCode").GetInt32().Should().Be(403);
        FirstMessage(envelope).Should().Be(MessageKey.Forbidden);
        context.Items.Should().ContainKey(nameof(Exception));
    }

    [Fact]
    public async Task Invoke_ShouldWrite404Envelope_ForNotFoundUserFriendlyException()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(new MilvaUserFriendlyException(MessageKey.UserNotFound)).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(404);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("statusCode").GetInt32().Should().Be(404);
        FirstMessage(envelope).Should().Be(MessageKey.UserNotFound);
    }

    [Fact]
    public async Task Invoke_ShouldWrite400Envelope_ForUserFriendlyExceptionWithUnknownKey()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(new MilvaUserFriendlyException("SomeBusinessRule")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        var envelope = await ReadEnvelopeAsync(context);
        FirstMessage(envelope).Should().Be("SomeBusinessRule");
    }

    [Fact]
    public async Task Invoke_ShouldFormatExceptionObjects_IntoTheLocalizedMessage()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(new MilvaUserFriendlyException(MessageKey.MaxLengthReached, new object[] { "Title", 200 })).Invoke(context);

        // Assert
        var envelope = await ReadEnvelopeAsync(context);
        FirstMessage(envelope).Should().Be(MessageKey.MaxLengthReached + "|Title,200");
    }

    [Fact]
    public async Task Invoke_ShouldKeepPreset401_AndUseUnauthorizedKey()
    {
        // Arrange: the auth layer already set 401 before a user-friendly exception surfaced.
        var context = CreateContext(presetStatus: 401);

        // Act
        await Middleware(new MilvaUserFriendlyException("whatever")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("statusCode").GetInt32().Should().Be(401);
        FirstMessage(envelope).Should().Be(MessageKey.Unauthorized);
    }

    [Fact]
    public async Task Invoke_ShouldKeepPreset403_AndUseTildeKeyFromMessage()
    {
        // Arrange: a "~Key" message overrides the default Forbidden key.
        var context = CreateContext(presetStatus: 403);

        // Act
        await Middleware(new MilvaUserFriendlyException("~" + MessageKey.Locked)).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
        FirstMessage(await ReadEnvelopeAsync(context)).Should().Be(MessageKey.Locked);
    }

    [Fact]
    public async Task Invoke_ShouldKeep419_AndUseSessionTimeoutKey()
    {
        // Arrange
        var context = CreateContext(presetStatus: StatusCodes.Status419AuthenticationTimeout);

        // Act
        await Middleware(new MilvaUserFriendlyException("ignored")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(419);
        FirstMessage(await ReadEnvelopeAsync(context)).Should().Be(MessageKey.SessionTimeout);
    }

    [Fact]
    public async Task Invoke_ShouldWrite409_ForPostgresUniqueViolation()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(Postgres("23505")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("statusCode").GetInt32().Should().Be(409);
        FirstMessage(envelope).Should().Be(MessageKey.PostgreDuplicateDataException);
    }

    [Fact]
    public async Task Invoke_ShouldWrite409_ForConfiguredForeignKeyViolationCode()
    {
        // Arrange: whatever code MessageConstant declares as the FK violation is answered with 409.
        var context = CreateContext();

        // Act
        await Middleware(Postgres(MessageConstant.ForeignKeyViolationPgCode)).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        FirstMessage(await ReadEnvelopeAsync(context)).Should().Be(MessageKey.PostgreBasedException);
    }

    [Fact]
    public async Task Invoke_ShouldWrite409_ForRealPostgresForeignKeyViolation()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(Postgres("23503")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Invoke_ShouldWrite409_ForSeedProtectionTrigger()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(Postgres(MessageConstant.DefaultDataCannotModifyPgCode, MessageConstant.DefaultDataCannotModifyPgMessage + " modified")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        FirstMessage(await ReadEnvelopeAsync(context)).Should().Be(MessageKey.DefaultValueCannotModify);
    }

    [Fact]
    public async Task Invoke_ShouldWrite500_ForOtherPostgresErrors()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(Postgres("42P01")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("statusCode").GetInt32().Should().Be(500);
        FirstMessage(envelope).Should().Be(MessageKey.PostgreBasedException);
    }

    [Fact]
    public async Task Invoke_ShouldUnwrapPostgresInnerException_FromGenericException()
    {
        // Arrange: EF wraps provider errors in DbUpdateException-like exceptions with the PostgresException inside.
        var context = CreateContext();

        // Act
        await Middleware(new InvalidOperationException("outer", Postgres("23505"))).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(409);
        FirstMessage(await ReadEnvelopeAsync(context)).Should().Be(MessageKey.PostgreDuplicateDataException);
    }

    [Fact]
    public async Task Invoke_ShouldWrite500_ForGenericException()
    {
        // Arrange
        var context = CreateContext();

        // Act
        await Middleware(new InvalidOperationException("boom")).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        var envelope = await ReadEnvelopeAsync(context);
        envelope.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
        envelope.GetProperty("statusCode").GetInt32().Should().Be(500);
        FirstMessage(envelope).Should().Be(MessageKey.UnhandledException);
    }

    [Fact]
    public async Task Invoke_ShouldLogRequestWithRedactedSensitiveHeaders_WhenLoggerEnabled()
    {
        // Arrange
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = new LoggerFactory([provider]);
        var context = CreateContext();

        context.Request.Headers.Authorization = "Bearer super-secret-token";
        context.Request.Headers.Cookie = "session=abc123";
        context.Request.Headers["X-Client"] = "unit-test";

        // Act
        await Middleware(new InvalidOperationException("boom"), loggerFactory).Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        provider.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error);

        var logged = provider.Entries.Single(e => e.Level == LogLevel.Error).Message;

        // The request info is a structured property, so only its absence of secrets is observable on the message.
        logged.Should().NotContain("super-secret-token");
        logged.Should().NotContain("abc123");
        logged.Should().Contain("boom");
    }

    [Theory]
    [InlineData("Authorization")]
    [InlineData("authorization")]
    [InlineData("Proxy-Authorization")]
    [InlineData("Cookie")]
    [InlineData("Set-Cookie")]
    public void RedactHeaders_ShouldMaskSensitiveHeaders_CaseInsensitively(string headerName)
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            [headerName] = "secret-value",
            ["X-Client"] = "unit-test"
        };

        // Act
        var redacted = RedactHeaders(headers);

        // Assert
        redacted[headerName].Should().Be("***");
        redacted["X-Client"].Should().Be("unit-test");
        redacted.Values.Should().NotContain("secret-value");
    }

    [Fact]
    public void RedactHeaders_ShouldJoinMultiValueHeaders()
    {
        // Arrange
        var headers = new HeaderDictionary { ["Accept"] = new Microsoft.Extensions.Primitives.StringValues(["a", "b"]) };

        // Act
        var redacted = RedactHeaders(headers);

        // Assert
        redacted["Accept"].Should().Be("a,b");
    }

    private static Dictionary<string, string> RedactHeaders(IHeaderDictionary headers)
    {
        var method = typeof(ExceptionMiddleware).GetMethod("RedactHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method.Should().NotBeNull("the middleware redacts headers in a private static RedactHeaders(IHeaderDictionary)");

        return (Dictionary<string, string>)method.Invoke(null, [headers]);
    }

    [Fact]
    public async Task Invoke_ShouldNotLog_ForConstraintViolations()
    {
        // Arrange: duplicate data is the caller's problem, not an incident worth an error log line.
        var provider = new CapturingLoggerProvider();
        using var loggerFactory = new LoggerFactory([provider]);
        var context = CreateContext();

        // Act
        await Middleware(Postgres("23505"), loggerFactory).Invoke(context);

        // Assert
        provider.Entries.Should().NotContain(e => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Invoke_ShouldSkipNext_WhenResponseAlreadyStarted()
    {
        // Arrange: when a response already started nothing is rewritten (would throw mid-stream).
        var context = CreateContext();
        var feature = new StartedResponseFeature(context.Response.Body);
        context.Features.Set<IHttpResponseFeature>(feature);
        var nextCalled = false;
        var middleware = new ExceptionMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, NullLoggerFactory.Instance);

        // Act
        await middleware.Invoke(context);

        // Assert
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task RewriteResponseAsync_ShouldDoNothing_WhenResponseHasStarted()
    {
        // Arrange
        var context = CreateContext();
        var body = context.Response.Body;
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature(body));

        // Act
        await ExceptionMiddleware.RewriteResponseAsync(context, MessageKey.Forbidden, 403);

        // Assert
        body.Length.Should().Be(0);
    }

    /// <summary>Response feature reporting <c>HasStarted = true</c>.</summary>
    private sealed class StartedResponseFeature(Stream body) : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = 200;

        public string ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = body;

        public bool HasStarted => true;

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }
    }
}
