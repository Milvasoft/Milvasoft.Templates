using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Milvasoft.Components.OpenApi;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.MultiLanguage.Builder;
using Milvasoft.Identity.Builder;
using Milvasoft.Localization.Builder;
using Milvasoft.Localization.Resx;
using Milvonion.Api.Controllers;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Domain;
using Milvonion.Infrastructure.Logging;
using Milvonion.Infrastructure.Utils.OpenApi;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.OpenTelemetry;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Milvonion.Api.AppStartup;

public static partial class StartupExtensions
{
    /// <summary>
    /// Configuration key of the access token signing key (HMAC-SHA256).
    /// </summary>
    public const string TokenSigningKeyConfigurationKey = "Milvasoft:Identity:Token:SymmetricPublicKey";

    /// <summary>
    /// Shortest signing key HMAC-SHA256 accepts, in UTF-8 bytes (256 bits).
    /// </summary>
    public const int MinTokenSigningKeyLength = 32;

    /// <summary>
    /// Makes sure the access token signing key (<see cref="TokenSigningKeyConfigurationKey"/>) is configured before the
    /// identity services read it. The key is a secret, so the appsettings files leave it empty: a shared environment sets
    /// it through the environment (<c>Milvasoft__Identity__Token__SymmetricPublicKey</c>), a developer through user secrets.
    /// Outside Development a missing or too short key stops the startup with a message that names the setting, instead of
    /// a key length error from the token library or, worse, a key everybody knows. In Development a random key is
    /// generated for this run and a warning is written, so a fresh clone (and <c>dotnet ef</c>) starts without setup; the
    /// tokens issued with it stop working on the next start.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder EnsureTokenSigningKey(this WebApplicationBuilder builder)
    {
        var signingKey = builder.Configuration[TokenSigningKeyConfigurationKey];

        if (!string.IsNullOrWhiteSpace(signingKey) && Encoding.UTF8.GetByteCount(signingKey) >= MinTokenSigningKeyLength)
            return builder;

        if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException($"The access token signing key '{TokenSigningKeyConfigurationKey}' is not set or is shorter than {MinTokenSigningKeyLength} characters. "
                                                + $"Set the environment variable '{TokenSigningKeyConfigurationKey.Replace(":", "__")}' to a random value of at least {MinTokenSigningKeyLength} characters (e.g. the output of 'openssl rand -hex 32').");
        }

        var generatedKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(MinTokenSigningKeyLength));

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> { [TokenSigningKeyConfigurationKey] = generatedKey });

        // Serilog is not built yet at this point of the startup, so the warning goes to the console.
        Console.WriteLine($"[Warning] '{TokenSigningKeyConfigurationKey}' is not set: a random signing key is used for this run, issued tokens stop working on restart. "
                          + $"Set a stable one with: dotnet user-secrets set \"{TokenSigningKeyConfigurationKey}\" \"<random value of at least {MinTokenSigningKeyLength} characters>\" (see README).");

        return builder;
    }

    /// <summary>
    /// Adds authorization services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuthorization(this IServiceCollection services, IConfigurationManager configuration)
    {
        var identityBuilder = services.AddMilvaIdentity<User, int>(configuration)
                                      .WithOptions()
                                      .WithDefaultTokenManager()
                                      .WithDefaultUserManager();

        services.AddSingleton(identityBuilder.IdentityOptions);

        // Deny-by-default: an endpoint that carries no authorization metadata at all requires an authenticated
        // caller, so a new controller cannot be exposed by forgetting [Auth]. Anonymous endpoints opt out
        // explicitly with [AllowAnonymous] (Account login/refresh, HealthCheck, UI localized contents).
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });

        // 403 from a role/permission policy carries the JSON envelope like every other failure.
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, EnvelopeAuthorizationResultHandler>();

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;

            // The API validates its own tokens (signing key, issuer and audience from Milvasoft:Identity:Token). No
            // Authority: it would make the handler download OpenID metadata from the issuer value on the first request.
            options.TokenValidationParameters = identityBuilder.IdentityOptions.Token.TokenValidationParameters;

            options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(60);

            options.Events = new JwtBearerEvents
            {
                // This event is fired when the token is not provided or after OnForbidden and OnAuthenticationFailed events.
                OnChallenge = context =>
                {
                    // We will add this check and response rewrite when the token is not provided.
                    // At the same time, since I set the response code in the OnForbidden and OnAuthenticationFailed events, it was added in order not to rewrite the response a second time.
                    if (!(context.Response.StatusCode == StatusCodes.Status403Forbidden || context.Response.StatusCode == StatusCodes.Status401Unauthorized))
                    {
                        // Since this scenario will work when a token is not sent to an endpoint that requires authorization, I set the response to 401.
                        context.HttpContext.Response.ThrowWithUnauthorized();
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    // Following if statement is redirects OnAuthenticationFailed again on 419.
                    if (context.Response.StatusCode is StatusCodes.Status419AuthenticationTimeout or StatusCodes.Status401Unauthorized
                        || AccountController.LoginEndpointPaths.Exists(e => context.Request.Path.Value.EndsWith(e)))
                        return Task.CompletedTask;

                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = StatusCodes.Status419AuthenticationTimeout;
                        throw new MilvaUserFriendlyException();
                    }
                    else
                    {
                        // Invalid token
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        throw new MilvaUserFriendlyException();
                    }
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Adds api versioning.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(x =>
        {
            x.AssumeDefaultVersionWhenUnspecified = true;
            x.DefaultApiVersion = ApiVersion.Default;
            x.ReportApiVersions = true;
            x.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"),
                                                          new HeaderApiVersionReader("api-version"),
                                                          new UrlSegmentApiVersionReader());
        }).AddApiExplorer(x =>
        {
            x.GroupNameFormat = "'v'V";
            x.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds openapi services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenApi(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddXmlComponentsForOpenApi(assemblies);

        services.AddOpenApi(GlobalConstant.DefaultApiVersion, options =>
        {
            // Specify the OpenAPI version to use
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            options.AddDocumentTransformer<ApiInfoTransformer>();
            options.AddSchemaTransformer<ExampleSchemaTransformer>();

            options.AddMilvaTransformers();

            // After the Milvasoft enum transformer: fills the enums it misses (built from a nullable property).
            options.AddSchemaTransformer<NullableEnumSchemaTransformer>();
        });

        return services;
    }

    /// <summary>
    /// Adds brotli and gzip response compression.
    /// </summary>
    /// <param name="services"></param>
    public static void AddAndConfigureResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            // The API sits behind a reverse proxy that terminates TLS and forwards X-Forwarded-Proto: https.
            // UseForwardedHeaders rewrites the scheme before the response body is written, so without this the
            // compression middleware sees an HTTPS request and skips compression entirely (the default is false).
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
    }

    /// <summary>
    /// Adds milva multi language services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddMultiLanguageSupport(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddMilvaLocalization(configuration)
                .WithResxManager<SharedResource>()
                .PostConfigureResxLocalizationOptions(opt =>
                {
                    opt.ResourcesPath = Path.Combine(GlobalConstant.LocalizationResourcesFolderName, GlobalConstant.ResourcesFolderName);
                    opt.ResourcesFolderPath = Path.Combine(Environment.CurrentDirectory, GlobalConstant.LocalizationResourcesFolderName, GlobalConstant.ResourcesFolderName);
                });

        services.AddMilvaMultiLanguage()
                .WithDefaultMultiLanguageManager();
    }

    /// <summary>
    /// Adds Serilog logging with environment specific sinks and OpenTelemetry metrics and tracing. The environment comes
    /// from <c>MILVA_ENV</c> (dev, test, prod). OpenTelemetry is registered only when <c>OpenTelemetry:CollectorUrl</c> is set
    /// and Seq is written only when <c>SeqUri</c> is set, so an empty configuration still starts the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddObservabilityAndLogging(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration.GetSection("OpenTelemetry:ServiceName").Get<string>();
        var collectorUrl = builder.Configuration.GetSection("OpenTelemetry:CollectorUrl").Get<string>();
        var seqUri = builder.Configuration.GetSection("SeqUri").Get<string>();
        var environment = MilvonionExtensions.GetCurrentEnvironment();

        if (string.IsNullOrWhiteSpace(serviceName))
            serviceName = GlobalConstant.ActivitySource.Name;

        // Serilog's own failures (e.g. an unreachable sink) are written to the console.
        SelfLog.Enable(msg =>
        {
            Console.WriteLine($"[Serilog.SelfLog] {msg}");
        });

        builder.Host.UseSerilog((_, lc) => lc.ReadFrom.Configuration(builder.Configuration).ApplyEnvironmentSpecificLogging(environment, serviceName, collectorUrl, seqUri));

        // Without a collector (local development, tests) there is nowhere to export to, so metrics and traces are not collected.
        if (string.IsNullOrWhiteSpace(collectorUrl))
            return builder;

        KeyValuePair<string, object>[] resourceAttributes =
        [
            new("deployment_environment", environment),
            new("instance_id", serviceName),
            new("host_name", serviceName)
        ];

        builder.Services.AddOpenTelemetry()
                        .ConfigureResource(resource => resource.AddService(serviceName).AddAttributes(resourceAttributes))
                        .WithMetrics(metricBuilder =>
                        {
                            metricBuilder.AddMeter(serviceName)
                                         .SetExemplarFilter(ExemplarFilterType.TraceBased)
                                         .AddAspNetCoreInstrumentation()
                                         .AddRuntimeInstrumentation()
                                         .AddHttpClientInstrumentation()
                                         .AddProcessInstrumentation()
                                         .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                                         {
                                             exporterOptions.Endpoint = new Uri(collectorUrl);
                                             exporterOptions.Protocol = OtlpExportProtocol.Grpc;
                                             metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
                                         });
                        })
                        .WithTracing(tracingBuilder =>
                        {
                            tracingBuilder.AddSource(GlobalConstant.ActivitySource.Name)
                                          .AddAspNetCoreInstrumentation(options =>
                                          {
                                              options.RecordException = true;
                                          })
                                          .AddHttpClientInstrumentation()
                                          .AddOtlpExporter(exporterOptions =>
                                          {
                                              exporterOptions.Endpoint = new Uri(collectorUrl);
                                              exporterOptions.Protocol = OtlpExportProtocol.Grpc;
                                          });
                        });

        return builder;
    }

    /// <summary>
    /// Adds the sinks of the current environment: dev writes to the console, test to Seq, prod to the OpenTelemetry
    /// collector and Seq. An environment whose sink is not configured (or an unknown environment) falls back to the console.
    /// </summary>
    /// <param name="loggerConfig"></param>
    /// <param name="environment"></param>
    /// <param name="serviceName"></param>
    /// <param name="collectorUrl"></param>
    /// <param name="seqUri"></param>
    /// <returns></returns>
    private static LoggerConfiguration ApplyEnvironmentSpecificLogging(this LoggerConfiguration loggerConfig, string environment, string serviceName, string collectorUrl, string seqUri)
    {
        var seqConfigured = !string.IsNullOrWhiteSpace(seqUri);
        var collectorConfigured = !string.IsNullOrWhiteSpace(collectorUrl);

        switch (environment.ToLowerInvariant())
        {
            case "test" when seqConfigured:
                loggerConfig.WriteTo.Seq(seqUri);
                break;
            case "prod" when seqConfigured || collectorConfigured:
                if (collectorConfigured)
                    loggerConfig.WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = collectorUrl;
                        options.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField | IncludedData.SourceContextAttribute;
                        options.ResourceAttributes = new Dictionary<string, object>
                        {
                            { "service_name", serviceName },
                            { "deployment_environment", environment }
                        };
                    });

                if (seqConfigured)
                    loggerConfig.WriteTo.Seq(seqUri);

                break;
            default:
                loggerConfig.WriteTo.Console();
                break;
        }

        // The service name identifies the app in the log store; the environment comes from MILVA_ENV like the
        // OpenTelemetry resource.
        loggerConfig.Enrich.WithProperty("AppName", serviceName)
                    .Enrich.WithProperty("Environment", environment)
                    .Enrich.With(new RemoveTypeTagEnricher());

        return loggerConfig;
    }
}