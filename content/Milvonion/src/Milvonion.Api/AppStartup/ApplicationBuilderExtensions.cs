using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.Identity.Concrete.Options;
using Milvasoft.Identity.TokenProvider.AuthToken;
using Milvasoft.Localization;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.PermissionManager;
using Milvonion.Domain.Enums;
using Milvonion.Infrastructure.Persistence.Context;
using Scalar.AspNetCore;
using System.Globalization;
using System.Security.Claims;

namespace Milvonion.Api.AppStartup;

/// <summary>
/// Application builder and service collection extensions.
/// </summary>
public static partial class StartupExtensions
{
    /// <summary>
    /// Adds the required middleware to use the localization. Configures the options before add..
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSwagger(this WebApplication app)
    {
        app.UseSwagger(c =>
        {
            c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            c.RouteTemplate = GlobalConstant.RoutePrefix + "/docs/{documentName}/docs.json";
        });

        app.MapScalarApiReference(endpointPrefix: $"/{GlobalConstant.RoutePrefix}/documentation", options =>
        {
            options.WithOpenApiRoutePattern($"/{GlobalConstant.RoutePrefix}/docs/v1.0/docs.json");
            options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
            options.AddApiKeyAuthentication(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                opt.Value = GenerateTokenForUI(app);
            });

            //UI
            options.WithTitle("Milvonion Api Reference")
                   .WithFavicon("https://demo.milvasoft.com/api/favicon.ico")
                   .WithDownloadButton(false)
                   .WithDarkMode(true)
                   .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                   .WithCustomCss(".darklight-reference-promo { display: none !important; } .darklight-reference { padding-bottom: 15px !important; } .open-api-client-button { display: none !important; }");
        });

        return app;

        static string GenerateTokenForUI(WebApplication app)
        {
            var identityOptions = app.Services.GetRequiredService<MilvaIdentityOptions>();

            var tokenManager = new MilvaTokenManager(identityOptions, null);

            var roleClaim = new Claim(ClaimTypes.Role, PermissionCatalog.App.SuperAdmin);
            var userTypeClaim = new Claim(GlobalConstant.UserTypeClaimName, UserType.Manager.ToString());
            var userClaim = new Claim(ClaimTypes.Name, "rootuser");

            var accessToken = tokenManager.GenerateToken(expired: DateTime.UtcNow.AddYears(1), issuer: null, userClaim, roleClaim, userTypeClaim);

            return "Bearer " + accessToken;
        }
    }

    /// <summary>
    /// Adds the required middleware to use the localization. Configures the options before add..
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRequestLocalization(this WebApplication app)
    {
        var supportedCultures = LanguagesSeed.Seed.Select(i => new CultureInfo(i.Code)).ToArray();

        var defaultLanguageCode = LanguagesSeed.Seed.First(l => l.IsDefault).Code;

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultLanguageCode),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            ApplyCurrentCultureToResponseHeaders = true
        };

        var defaultCulture = new CultureInfo(defaultLanguageCode);

        CultureInfo.CurrentCulture = defaultCulture;
        CultureInfo.CurrentUICulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentCulture = defaultCulture;

        _ = new CultureSwitcher(defaultLanguageCode);

        return app.UseRequestLocalization(options);
    }

    /// <summary>
    /// Adds the required middleware to use the health check. Configures the options before adding.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseHealthCheck(this WebApplication app)
    {
        app.UseHealthChecks($"/{GlobalConstant.RoutePrefix}/{GlobalConstant.HealthCheckPath}", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(delegate (Options options)
        {
            options.UseRelativeApiPath = false;
            options.UseRelativeResourcesPath = false;
            options.UseRelativeWebhookPath = false;
            options.UIPath = $"/{GlobalConstant.RoutePrefix}/{GlobalConstant.HealthCheckUIPath}";
            options.ApiPath = $"{options.UIPath}/{GlobalConstant.RoutePrefix}";
            options.ResourcesPath = $"/{GlobalConstant.RoutePrefix}/{GlobalConstant.HealthCheckResourcesPath}";
            options.WebhookPath = $"{options.UIPath}/{GlobalConstant.WWWRoot}";
            options.AddCustomStylesheet(Path.Combine(Directory.GetCurrentDirectory(), GlobalConstant.WWWRoot, GlobalConstant.HealthCheckResourcesPath, "hc.css"));
        });

        return app;
    }

    /// <summary>
    /// Adds the required middleware to serve static files. Configures the options before adding.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder UseStaticFiles(this WebApplication app)
    {
        app.UseStaticFiles($"/{GlobalConstant.RoutePrefix}");

        return app;
    }

    /// <summary>
    /// Creates database if not exists. 
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    public static async Task<IApplicationBuilder> CreateDatabaseIfNotExistsAsync(this WebApplication app)
    {
        using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
        {
            var factory = serviceScope.ServiceProvider.GetRequiredService<MilvonionDbContextScopedFactory>();
            await factory.CreateDbContext().Database.MigrateAsync();
        }

        return app;
    }
}
