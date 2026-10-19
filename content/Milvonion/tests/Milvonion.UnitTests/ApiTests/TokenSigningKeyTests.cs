using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Milvonion.Api.AppStartup;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="StartupExtensions.EnsureTokenSigningKey"/>: the appsettings files carry no signing key, so outside Development
/// the startup stops without one and in Development a random key is generated for the run.
/// </summary>
[Trait("Api Unit Tests", "Access token signing key check at startup.")]
public class TokenSigningKeyTests
{
    [Theory]
    [InlineData("Production", "")]
    [InlineData("Staging", "")]
    [InlineData("Production", "too-short-key")]
    public void EnsureTokenSigningKey_OutsideDevelopmentWithoutAValidKey_ShouldStopWithAMessageNamingTheSetting(string environmentName, string signingKey)
    {
        // Arrange
        var builder = CreateBuilder(environmentName, signingKey);

        // Act
        var act = () => builder.EnsureTokenSigningKey();

        // Assert
        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("Milvasoft__Identity__Token__SymmetricPublicKey");
    }

    [Fact]
    public void EnsureTokenSigningKey_InDevelopmentWithoutAKey_ShouldUseARandomKeyOfTheRequiredLength()
    {
        // Arrange
        var firstBuilder = CreateBuilder("Development", string.Empty);
        var secondBuilder = CreateBuilder("Development", string.Empty);

        // Act
        firstBuilder.EnsureTokenSigningKey();
        secondBuilder.EnsureTokenSigningKey();

        // Assert
        var firstKey = firstBuilder.Configuration[StartupExtensions.TokenSigningKeyConfigurationKey];
        var secondKey = secondBuilder.Configuration[StartupExtensions.TokenSigningKeyConfigurationKey];

        firstKey.Should().MatchRegex($"^[0-9A-F]{{{StartupExtensions.MinTokenSigningKeyLength * 2}}}$");
        secondKey.Should().NotBe(firstKey);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Development")]
    public void EnsureTokenSigningKey_WithAConfiguredKey_ShouldKeepIt(string environmentName)
    {
        // Arrange
        var signingKey = new string('k', StartupExtensions.MinTokenSigningKeyLength);
        var builder = CreateBuilder(environmentName, signingKey);

        // Act
        builder.EnsureTokenSigningKey();

        // Assert
        builder.Configuration[StartupExtensions.TokenSigningKeyConfigurationKey].Should().Be(signingKey);
    }

    /// <summary>
    /// A builder in an empty content root (no appsettings file) whose signing key is <paramref name="signingKey"/>, whatever
    /// the machine's environment variables or user secrets say.
    /// </summary>
    private static WebApplicationBuilder CreateBuilder(string environmentName, string signingKey)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName,
            ContentRootPath = Directory.CreateTempSubdirectory().FullName
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string> { [StartupExtensions.TokenSigningKeyConfigurationKey] = signingKey });

        return builder;
    }
}
