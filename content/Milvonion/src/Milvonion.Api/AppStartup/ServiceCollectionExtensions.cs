using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Milvasoft.Components.Swagger.DocumentFilters;
using Milvasoft.Components.Swagger.OperationFilters;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.MultiLanguage.Builder;
using Milvasoft.Identity.Builder;
using Milvasoft.Localization.Builder;
using Milvasoft.Localization.Resx;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Domain;
using Milvonion.Infrastructure.Utils.Swagger;
using System.IO.Compression;
using System.Net;
using System.Reflection;

namespace Milvonion.Api.AppStartup;

public static partial class StartupExtensions
{
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

        services.AddAuthorization();

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.Authority = identityBuilder.IdentityOptions.Token.TokenValidationParameters.ValidIssuer;
            options.TokenValidationParameters = identityBuilder.IdentityOptions.Token.TokenValidationParameters;

            options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(60);

            options.Events = new JwtBearerEvents
            {
                // This event is fired when the token is not provided or after OnForbidden and OnAuthenticationFailed events.
                OnChallenge = context =>
                {
                    // We will add this check and response rewrite when the token is not provided.
                    // At the same time, since I set the response code in the OnForbidden and OnAuthenticationFailed events, it was added in order not to rewrite the response a second time.
                    if (!(context.Response.StatusCode == 403 || context.Response.StatusCode == 401))
                    {
                        // Since this scenario will work when a token is not sent to an endpoint that requires authorization, I set the response to 401.
                        context.Response.ThrowWithUnauthorized();
                    }

                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    // Invalid permissions
                    context.Response.StatusCode = 403;
                    throw new MilvaUserFriendlyException();
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.ThrowWithUnauthorized();
                    // Invalid token
                    context.Response.StatusCode = 401;
                    throw new MilvaUserFriendlyException();
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
    /// Adds swagger services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = MessageConstant.SwaggerAuthMessageTip,
                Name = nameof(Authorization),
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Scheme = GlobalConstant.Http,
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            options.SwaggerDoc(GlobalConstant.DefaultApiVersion, new OpenApiInfo
            {
                Version = GlobalConstant.DefaultApiVersion,
                Title = "Milvonion Api",
                Description = "Milvonion api.",
                TermsOfService = new Uri("https://www.milvasoft.com"),
                Contact = new OpenApiContact { Name = "Milvonion", Email = "info@milvasoft.com", Url = new Uri("https://www.milvasoft.com") },
                License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
            });

            foreach (var assembly in assemblies)
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            }

            options.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
            options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
            options.OperationFilter<SwaggerFileOperationFilter>();
            options.OperationFilter<RequestHeaderFilter>();
            options.SchemaFilter<ExampleSchemaFilter>();
            options.SchemaFilter<EnumSchemaFilter>();
        });

        return services;
    }

    /// <summary>
    /// Adds health check services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(2000, "Memory")
                .AddNpgSql(
                    connectionString: configuration.GetConnectionString("DefaultConnectionString"),
                    healthQuery: "SELECT 1;",
                    name: "PostgreSQL-DefaultConnectionString",
                    failureStatus: HealthStatus.Degraded,
                    timeout: TimeSpan.FromSeconds(30),
                    tags: ["db", "sql", "postgres"]
                );

        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        var endpoint = string.IsNullOrEmpty(urls) ? configuration.GetSection("ApiHost:HttpUrl").Get<string>() : urls.Split(';')[0];

        endpoint = endpoint.Replace("+", Dns.GetHostName()).Replace("*", Dns.GetHostName()).Replace("::", Dns.GetHostName());

        services.AddHealthChecksUI(setup =>
        {
            setup.AddHealthCheckEndpoint("HealthChecks", $"{endpoint}/{GlobalConstant.RoutePrefix}/{GlobalConstant.HealthCheckPath}");
        }).AddInMemoryStorage();
    }

    /// <summary>
    /// Adds brotli and gzip response compression.
    /// </summary>
    /// <param name="services"></param>
    public static void AddAndConfigureResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
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
}