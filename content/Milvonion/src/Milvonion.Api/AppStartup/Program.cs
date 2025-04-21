using Milvasoft.Components.Rest;
using Milvasoft.Core.Utils.Converters;
using Milvonion.Api;
using Milvonion.Api.AppStartup;
using Milvonion.Api.Middlewares;
using Milvonion.Api.Migrations;
using Milvonion.Application;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.LinkedWithFormatters;
using Milvonion.Domain;
using Milvonion.Infrastructure;
using Serilog;
using Serilog.Debugging;
using System.Reflection;
#if !DEBUG
using Milvonion.Infrastructure.Logging;
#endif

try
{
#if DEBUG
    MissingResxKeyFinder.FindAndPrintToConsole();
#endif

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        WebRootPath = GlobalConstant.WWWRoot
    });

    var assemblies = new Assembly[] { ApplicationAssembly.Assembly, InfrastructureAssembly.Assembly, DomainAssembly.Assembly, PresentationAssembly.Assembly };

    // Serilog logs to console.
    SelfLog.Enable(Console.Error);

#if DEBUG
    builder.Host.UseSerilog((_, lc) => lc.ReadFrom.Configuration(builder.Configuration));
#else
    builder.Host.UseSerilog((_, lc) => lc.ReadFrom.Configuration(builder.Configuration).Enrich.With(new RemoveTypeTagEnricher()));
#endif

    #region ConfigureServices

    // Add services to the container.
    var services = builder.Services;

    services.AddControllers();

    services.AddEndpointsApiExplorer();

    services.AddVersioning();

    services.AddSwagger(assemblies);

#if !DEBUG
    services.AddHealthCheck(builder.Configuration);
#endif

    services.AddAuthorization(builder.Configuration);

    services.AddHttpContextAccessor();

    services.AddMultiLanguageSupport(builder.Configuration);

    services.ConfigureCurrentMilvaJsonSerializerOptions().AddResponseConverters();

    services.AddAndConfigureResponseCompression();

    services.AddApplicationServices(assemblies);

    services.AddInfrastructureServices(builder.Configuration);

    services.AddLinkedWithFormatters(assemblies);

    services.AddFetchers();

    services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });

    builder.Services.AddHostedService<MigrationHostedService>();

    #endregion

    services.AddCors(o => o.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    }));

    // Configure the HTTP request pipeline.
    var app = builder.Build();

    app.UseStaticFiles();

    app.UseCors("AllowAll");

    #region Configure

    // This must be first.
    app.UseResponseCompression();

    app.UseRequestLocalization();

    //app.UseMiddleware<RequestResponseLoggingMiddleware>();

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.UseSwagger();

#if !DEBUG
    app.UseHealthCheck();
#endif

    #endregion

    await app.RunAsync();

}
catch (Exception ex)
{
    Log.Logger.Error(ex, "Error ");
    Console.WriteLine(ex.Message);
}