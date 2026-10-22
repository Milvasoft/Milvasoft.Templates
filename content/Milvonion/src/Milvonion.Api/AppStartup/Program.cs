using Microsoft.AspNetCore.HttpOverrides;
using Milvasoft.Components.Rest;
using Milvasoft.Core.Utils.Converters;
using Milvonion.Api;
using Milvonion.Api.AppStartup;
using Milvonion.Api.Middlewares;
using Milvonion.Api.Migrations;
using Milvonion.Api.Utils;
using Milvonion.Application;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.LinkedWithFormatters;
using Milvonion.Domain;
using Milvonion.Infrastructure;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

try
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        WebRootPath = GlobalConstant.WWWRoot
    });

    var assemblies = new Assembly[] { ApplicationAssembly.Assembly, InfrastructureAssembly.Assembly, DomainAssembly.Assembly, PresentationAssembly.Assembly };

    builder.AddObservabilityAndLogging();

    #region ConfigureServices

    // Add services to the container.
    var services = builder.Services;

    // Failed envelopes leave with a real HTTP status (see ResponseStatusCodeFilter) instead of 200.
    services.AddControllers(options => options.Filters.Add<ResponseStatusCodeFilter>())
            .AddJsonOptions(options =>
            {
                // A null property carries no information, only bytes. Clients read a missing property and an explicit
                // null the same way, and requests are unaffected: this is a write-side setting only.
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                // Dates (e.g. a date of birth) are read by the date they were written with, never through a time zone.
                options.JsonSerializerOptions.Converters.Add(new LenientDateOnlyJsonConverter());
            });

    services.AddEndpointsApiExplorer();

    services.AddVersioning();

    services.AddOpenApi(assemblies);

    // The signing key is a secret and not in the appsettings files: fail fast outside Development, generate one in Development.
    builder.EnsureTokenSigningKey();

    services.AddAuthorization(builder.Configuration);

    services.AddHttpContextAccessor();

    services.AddMultiLanguageSupport(builder.Configuration);

    services.ConfigureCurrentMilvaJsonSerializerOptions().AddResponseConverters();

    services.AddAndConfigureResponseCompression();

    services.AddApplicationServices(assemblies);

    // Hosted services start in registration order: migrations must complete before the
    // infrastructure background services (scheduled cleanup jobs) touch the database.
    builder.Services.AddHostedService<MigrationHostedService>();

    services.AddInfrastructureServices(builder.Configuration);

    // Liveness (GET healthcheck) touches nothing; readiness (GET healthcheck/ready) checks the database and Redis.
    services.AddReadinessChecks();

    services.AddLinkedWithFormatters(assemblies);

    services.AddFetchers();

    services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });

    #endregion

    services.AddCors(o => o.AddPolicy("AllowAll", builder =>
    {
        // Content-Disposition is exposed so a client can save a downloaded file (e.g. an export) under its real name.
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(GlobalConstant.ContentDispositionHeaderKey);
    }));

    // Configure the HTTP request pipeline.
    var app = builder.Build();

    // Development only: reports localization keys missing from the resx files. Skipped with a warning when the source tree is not available (e.g. inside a container).
    if (app.Environment.IsDevelopment())
    {
        MissingResxKeyFinder.FindAndPrintToConsole();
    }

    app.UseStaticFiles();

    app.UseCors("AllowAll");

    #region Configure

    // This must be first.
    app.UseResponseCompression();

    app.UseRequestLocalization();

    // Behind the reverse proxy: rewrite RemoteIpAddress/Scheme from X-Forwarded-For / X-Forwarded-Proto so
    // logs and IP based checks see the client, not the proxy. Runs before authentication.
    // The proxy is not on loopback (container/ingress), so the default KnownProxies/KnownIPNetworks would ignore
    // the header entirely. Requests pass two proxies: Cloudflare (proxied DNS) and Traefik, which trusts only the
    // Cloudflare ranges and appends Cloudflare's address. ForwardLimit = 2 keeps the two RIGHTMOST
    // X-Forwarded-For entries, so the client address is the one Cloudflare wrote; values a client prepends are
    // ignored. With a single proxy in front, set it back to 1. Deploy behind a proxy only.
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 2
    };

    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();

    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseMiddleware<ExceptionMiddleware>();

    // Endpoint routing must run before authentication/authorization so the matched endpoint's metadata
    // ([AllowAnonymous], [Auth]) is visible to them; with the deny-by-default fallback policy an unresolved
    // endpoint would otherwise be treated as "requires authentication".
    app.UseRouting();

    // A request that matched no endpoint is a 404, not a 401: without this the deny-by-default fallback policy
    // below would challenge every unknown URL (the authorization middleware applies it when there is no endpoint).
    app.Use((context, next) => context.GetEndpoint() is null ? ExceptionMiddleware.RewriteResponseAsync(context, MessageKey.PageNotFound, StatusCodes.Status404NotFound) : next(context));

    app.UseAuthentication();

    // Token revocation: an authenticated request whose session stamp no longer matches the user's current one is a 401.
    app.UseMiddleware<SecurityStampValidationMiddleware>();

    app.UseAuthorization();

    app.MapControllers();

    app.UseScalarWithOpenApi();

    #endregion

    await app.RunAsync();

}
catch (Exception ex)
{
    Log.Logger.Error(ex, "Error ");
    Console.WriteLine(ex.Message);
}