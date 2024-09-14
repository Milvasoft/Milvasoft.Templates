using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Core.MultiLanguage.Manager;
using Milvasoft.Localization;
using Milvonion.Application.Utils.Constants;
using Milvonion.Infrastructure.Persistence.Context;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Globalization;

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
            c.SerializeAsV2 = true;
            c.RouteTemplate = GlobalConstant.RoutePrefix + "/docs/{documentName}/docs.json";
        }).UseSwaggerUI(c =>
        {
            c.DefaultModelExpandDepth(-1);
            c.DefaultModelsExpandDepth(1);
            c.DefaultModelRendering(ModelRendering.Example);
            c.DocExpansion(DocExpansion.None);
            c.RoutePrefix = $"{GlobalConstant.RoutePrefix}/documentation";
            c.SwaggerEndpoint($"/{GlobalConstant.RoutePrefix}/docs/v1.0/docs.json", "Milvonion Api v1.0");
            c.InjectJavascript($"/{GlobalConstant.RoutePrefix}/swagger-ui/custom.js");
        });

        return app;
    }

    /// <summary>
    /// Adds the required middleware to use the localization. Configures the options before add..
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRequestLocalization(this WebApplication app)
    {
        var supportedCultures = LanguagesSeed.Seed.Select(i => new CultureInfo(i.Code)).ToList();

        var defaultLanguageCode = LanguagesSeed.Seed.First(l => l.IsDefault).Code;

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(defaultLanguageCode),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            ApplyCurrentCultureToResponseHeaders = true
        };

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
