using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Milvonion.Api.AppStartup;
using Milvonion.UnitTests.TestSupport;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// Guards the committed appsettings files: they carry no secret and the security related defaults stay safe. Read with
/// the configuration builder, so the values are what the application binds.
/// </summary>
[Trait("Api Unit Tests", "Committed appsettings files.")]
public class AppSettingsTests
{
    public static TheoryData<string> AppSettingsFiles => ["appsettings.json", "appsettings.Development.json"];

    [Theory]
    [MemberData(nameof(AppSettingsFiles))]
    public void Secrets_ShouldNotBeCommitted(string fileName)
    {
        // Arrange
        var configuration = Load(fileName);

        // Act
        var signingKey = configuration[StartupExtensions.TokenSigningKeyConfigurationKey];
        var encryptionKey = configuration["Milvasoft:FileOperations:Json:EncryptionKey"];

        // Assert: see README "Secrets"; the keys come from user secrets or the environment, never from a committed file.
        signingKey.Should().BeEmpty();
        encryptionKey.Should().BeEmpty();
    }

    [Fact]
    public void ConnectionString_ShouldOnlyBeInTheDevelopmentFile()
    {
        // Arrange
        var configuration = Load("appsettings.json");
        var developmentConfiguration = Load("appsettings.Development.json");

        // Act
        var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        var developmentConnectionString = developmentConfiguration.GetConnectionString("DefaultConnectionString");

        // Assert: the --PostgreConnectionString value of dotnet new (a local database with its password) is not the default
        // of every environment; a deployment sets ConnectionStrings__DefaultConnectionString.
        connectionString.Should().BeEmpty();
        developmentConnectionString.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [MemberData(nameof(AppSettingsFiles))]
    public void TokenValidation_ShouldCheckIssuerAndAudience(string fileName)
    {
        // Arrange
        var configuration = Load(fileName);

        // Act
        var section = configuration.GetSection("Milvasoft:Identity:Token:TokenValidationParameters");

        // Assert: a token of another service that shares the signing key must not be valid here.
        section.GetValue<bool>("ValidateIssuer").Should().BeTrue();
        section.GetValue<bool>("ValidateAudience").Should().BeTrue();
        section["ValidIssuer"].Should().NotBeNullOrWhiteSpace();
        section["ValidAudience"].Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [MemberData(nameof(AppSettingsFiles))]
    public void MethodLogs_ShouldNotWriteArgumentsAndResultsByDefault(string fileName)
    {
        // Arrange
        var configuration = Load(fileName);

        // Act
        var logDefaultParameters = configuration.GetValue<bool>("Milvasoft:Interception:Log:LogDefaultParameters");

        // Assert: see README "Method logs"; arguments and results carry personal data and secrets.
        logDefaultParameters.Should().BeFalse();
    }

    /// <summary>
    /// Loads one appsettings file of the Api project from the sources; skips when the sources are not next to the binaries.
    /// </summary>
    private static IConfigurationRoot Load(string fileName)
    {
        if (SourceCode.SolutionRoot == null)
            Assert.Skip("The sources are not available.");

        var apiFolder = Path.Combine(SourceCode.SolutionRoot, "src", typeof(IApiMarker).Assembly.GetName().Name);

        return new ConfigurationBuilder().SetBasePath(apiFolder).AddJsonFile(fileName, optional: false).Build();
    }
}
