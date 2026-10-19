using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Identity.Concrete.Options;
using Milvonion.Application.Dtos.UserDtos;
using Milvonion.Application.Utils.Constants;
using Milvonion.IntegrationTests.TestBase;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the pipeline behaviours shared by every endpoint.")]
public class CrossCuttingTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UnknownUrl_Is404WithEnvelopeNot401()
    {
        // Act
        var httpResponse = await _factory.CreateClientWithAddress().GetAsync($"{TestApi.Base}/not-existing-endpoint", TestContext.Current.CancellationToken);
        var result = await TestApi.ReadAsync<Response>(httpResponse);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EndpointWithoutAuthorizationMetadata_WithoutToken_ShouldBe401WithEnvelope()
    {
        // Act
        var httpResponse = await _factory.CreateClientWithAddress().PostAsync($"{TestApi.Base}/developer/database/seed", null, TestContext.Current.CancellationToken);
        var result = await TestApi.ReadAsync<Response>(httpResponse);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AnonymousEndpoint_WithoutToken_ShouldBeReachable()
    {
        // Act
        var healthResponse = await _factory.CreateClientWithAddress().GetAsync($"{TestApi.Base}/healthcheck", TestContext.Current.CancellationToken);
        var localizedContentsResponse = await _factory.CreateClientWithAddress().GetAsync($"{TestApi.Base}/ui/localizedContents", TestContext.Current.CancellationToken);
        var documentResponse = await _factory.CreateClientWithAddress().GetAsync($"{GlobalConstant.RoutePrefix}/docs/{GlobalConstant.DefaultApiVersion}/docs.json", TestContext.Current.CancellationToken);

        // Assert
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        localizedContentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        documentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DetailOfMissingRecord_ShouldBe404()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (httpResponse, result) = await root.Client.GetAsync<UserDetailDto>("users/user?UserId=999999");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NoTestCanReachTheNetwork_EveryHttpClientIsProxiedToAClosedPort()
    {
        // Arrange: the factory routes HttpClient.DefaultProxy to 127.0.0.1:1, so an accidental real call (a provider
        // added later, telemetry over HTTP) fails at once instead of leaving the machine. Guards the guard.
        _ = _factory.Server;

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        // Act
        var act = () => client.GetAsync("https://example.com/", TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task TokenSignedWithTheSameKey_ForAnotherIssuerOrAudience_ShouldBe401()
    {
        // Arrange: the claims of a real token re-signed with this API's key. Only the issuer or the audience differs, as
        // with a token of another service (an admin API) that shares the signing key.
        var user = await _factory.CreateAndLoginUserAsync();

        var identityOptions = _factory.Services.GetRequiredService<MilvaIdentityOptions>();
        var validIssuer = identityOptions.Token.TokenValidationParameters.ValidIssuer;
        var validAudience = identityOptions.Token.TokenValidationParameters.ValidAudience;
        var signingKey = identityOptions.Token.GetSecurityKey();

        // Act
        var sameServiceResponse = await GetAccountDetailAsync(ResignToken(user.Token.AccessToken, validIssuer, validAudience, signingKey));
        var otherIssuerResponse = await GetAccountDetailAsync(ResignToken(user.Token.AccessToken, "OtherServiceHub", validAudience, signingKey));
        var otherAudienceResponse = await GetAccountDetailAsync(ResignToken(user.Token.AccessToken, validIssuer, "OtherServiceHub", signingKey));

        // Assert: the re-signed token itself is fine (control), the foreign issuer and audience are not.
        sameServiceResponse.Should().Be(HttpStatusCode.OK);
        otherIssuerResponse.Should().Be(HttpStatusCode.Unauthorized);
        otherAudienceResponse.Should().Be(HttpStatusCode.Unauthorized);
        validIssuer.Should().NotBeNullOrWhiteSpace();
        validAudience.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SigningKey_ShouldBeTheOneGivenThroughTheEnvironment()
    {
        // Arrange: the appsettings files carry no key; in Development the application would generate a random one, so a
        // token signed with the factory's key is only accepted when the environment variable reached the host.
        var user = await _factory.CreateAndLoginUserAsync();

        var identityOptions = _factory.Services.GetRequiredService<MilvaIdentityOptions>();
        var parameters = identityOptions.Token.TokenValidationParameters;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(MilvonionApiFactory.TokenSigningKey));

        // Act
        var response = await GetAccountDetailAsync(ResignToken(user.Token.AccessToken, parameters.ValidIssuer, parameters.ValidAudience, signingKey));

        // Assert
        identityOptions.Token.SymmetricPublicKey.Should().Be(MilvonionApiFactory.TokenSigningKey);
        response.Should().Be(HttpStatusCode.OK);
    }

    private async Task<HttpStatusCode> GetAccountDetailAsync(string accessToken)
    {
        var client = _factory.CreateClientWithAddress().UseAccessToken(accessToken);

        var response = await client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        return response.StatusCode;
    }

    private static string ResignToken(string accessToken, string issuer, string audience, SecurityKey signingKey)
    {
        var handler = new JwtSecurityTokenHandler();

        var source = handler.ReadJwtToken(accessToken);

        string[] registeredClaims = [JwtRegisteredClaimNames.Iss, JwtRegisteredClaimNames.Aud, JwtRegisteredClaimNames.Exp, JwtRegisteredClaimNames.Nbf, JwtRegisteredClaimNames.Iat];

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(source.Claims.Where(c => !registeredClaims.Contains(c.Type))),
            Issuer = issuer,
            Audience = audience,
            NotBefore = DateTime.UtcNow.AddMinutes(-1),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
