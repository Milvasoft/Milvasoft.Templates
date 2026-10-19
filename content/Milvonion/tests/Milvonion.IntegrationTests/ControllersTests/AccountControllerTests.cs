using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Identity.Abstract;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Features.Account.ChangePassword;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Features.Account.RefreshLogin;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.IntegrationTests.TestBase;
using System.Net;
using System.Security.Claims;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for controllers.")]
public class AccountControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    // PasswordHash values of the Identity v3 format: the first one is the hash of "string", the second one is no valid hash.
    private const string _stringPasswordHash = "AQAAAAEAACcQAAAAEIMgDN79W03UXM/6VMhU4ua5i6DRRmS/kj77Jdfi1vjxSUnugS7ZF1CszYSLuhKvpw==";
    private const string _invalidPasswordHash = "AQAAAAEAACcQAAAAEJ9";

    #region Login

    [Fact]
    public async Task LoginAsync_WithInvalidUsernameAndPassword_ShouldNotLogin()
    {
        // Arrange
        var request = new LoginCommand
        {
            UserName = "admin",
            Password = "admin",
            DeviceId = "device-id"
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_WithValidUsernameButInvalidPassword_ShouldNotLogin()
    {
        // Arrange
        var userName = await AddUserAsync(_invalidPasswordHash);

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = "device-id"
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_WithValidUsernameButInvalidPasswordAndFailedMaxLockoutCount_ShouldNotLoginAndLockUser()
    {
        // Arrange
        var userName = await AddUserAsync(_invalidPasswordHash, lockoutEnabled: true);

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = "device-id"
        };

        // Act
        var client = _factory.CreateClientWithAddress();
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        await client.PostAsync<LoginResponseDto>("account/login", request);
        await client.PostAsync<LoginResponseDto>("account/login", request);
        await client.PostAsync<LoginResponseDto>("account/login", request);

        var (beforeLockHttpResponse, beforeLockLoginResult) = await client.PostAsync<LoginResponseDto>("account/login", request);
        var userBeforeLockout = await _factory.GetUserRowAsync(userName);

        var (lockHttpResponse, lockLoginResult) = await client.PostAsync<LoginResponseDto>("account/login", request);
        var userOnLockout = await _factory.GetUserRowAsync(userName);

        var (afterLockHttpResponse, afterLockLoginResult) = await client.PostAsync<LoginResponseDto>("account/login", request);
        var userAfterLockout = await _factory.GetUserRowAsync(userName);

        // Arrange
        beforeLockHttpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        beforeLockLoginResult.Should().NotBeNull();
        beforeLockLoginResult.Messages[0].Message.Should().Be("The session will be locked after 1 invalid attempts.");
        beforeLockLoginResult.Data.Should().BeNull();
        beforeLockLoginResult.IsSuccess.Should().BeFalse();
        beforeLockLoginResult.IsCachedData.Should().BeFalse();
        beforeLockLoginResult.Metadatas.Should().BeEmpty();
        beforeLockLoginResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        userBeforeLockout.Should().NotBeNull();
        userBeforeLockout.LockoutEnabled.Should().BeTrue();
        userBeforeLockout.AccessFailedCount.Should().Be(4);
        userBeforeLockout.LockoutEnd.Should().BeNull();

        lockHttpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        lockLoginResult.Should().NotBeNull();
        lockLoginResult.Messages[0].Message.Should().Be("Your account has been locked due to repeated unsuccessful login attempts. Try again after 4 minutes.");
        lockLoginResult.Data.Should().BeNull();
        lockLoginResult.IsSuccess.Should().BeFalse();
        lockLoginResult.IsCachedData.Should().BeFalse();
        lockLoginResult.Metadatas.Should().BeEmpty();
        lockLoginResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        userOnLockout.Should().NotBeNull();
        userOnLockout.LockoutEnabled.Should().BeTrue();
        userOnLockout.AccessFailedCount.Should().Be(0);
        userOnLockout.LockoutEnd.Should().NotBeNull();

        afterLockHttpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        afterLockLoginResult.Should().NotBeNull();
        afterLockLoginResult.Messages[0].Message.Should().Be("Your account has been locked due to repeated unsuccessful login attempts. Try again after 4 minutes.");
        afterLockLoginResult.Data.Should().BeNull();
        afterLockLoginResult.IsSuccess.Should().BeFalse();
        afterLockLoginResult.IsCachedData.Should().BeFalse();
        afterLockLoginResult.Metadatas.Should().BeEmpty();
        afterLockLoginResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        userAfterLockout.Should().NotBeNull();
        userAfterLockout.LockoutEnabled.Should().BeTrue();
        userAfterLockout.AccessFailedCount.Should().Be(0);
        userAfterLockout.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithLockedUser_ShouldNotLogin()
    {
        // Arrange
        var userName = await AddUserAsync(_stringPasswordHash, lockoutEnabled: true, accessFailedCount: 25, lockoutEnd: DateTime.UtcNow.AddMinutes(10));

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = "device-id"
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginAsync_WithLockedUserButSuccessfullLogin_ShouldLoginAndResetLockout()
    {
        // Arrange
        var userName = await AddUserAsync(_stringPasswordHash, lockoutEnabled: true, accessFailedCount: 25, lockoutEnd: DateTime.UtcNow.AddMinutes(-1));

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = "device-id"
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().NotBeNull();
        loginResult.Data.UserType.Should().Be(UserType.AppUser);
        loginResult.Data.Token.Should().NotBeNull();
        loginResult.Data.Token.ExpiresIn.Should().NotBe(0);
        loginResult.Data.Token.RefreshTokenExpiresIn.Should().NotBe(0);
        loginResult.Data.Token.RefreshToken.Should().NotBeNull();
        loginResult.Data.Token.TokenType.Should().Be("Bearer");
        loginResult.Data.Token.Scope.Should().Be("Milvonion");
        loginResult.Data.Token.SessionState.Should().BeNull();
        loginResult.Data.AccessibleMenuItems.Should().BeNullOrEmpty();
        loginResult.Data.PageInformations.Should().BeNullOrEmpty();

        loginResult.IsSuccess.Should().BeTrue();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var user = await _factory.GetUserRowAsync(userName);
        user.Should().NotBeNull();
        user.LockoutEnabled.Should().BeTrue();
        user.LockoutEnd.Should().BeNull();
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public async Task LoginAsync_WithValidUsernameAndPasswordButInvalidDeviceId_ShouldNotLogin()
    {
        // Arrange
        var userName = await AddUserAsync(_invalidPasswordHash);

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = ""
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().BeNull();
        loginResult.IsSuccess.Should().BeFalse();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldLogin()
    {
        // Arrange
        var userName = await AddUserAsync(_stringPasswordHash);

        var request = new LoginCommand
        {
            UserName = userName,
            Password = "string",
            DeviceId = "device-id"
        };

        // Act
        var (httpResponse, loginResult) = await _factory.CreateClientWithAddress().PostAsync<LoginResponseDto>("account/login", request);

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().NotBeNull();
        loginResult.Data.UserType.Should().Be(UserType.AppUser);
        loginResult.Data.Token.Should().NotBeNull();
        loginResult.Data.Token.ExpiresIn.Should().NotBe(0);
        loginResult.Data.Token.RefreshTokenExpiresIn.Should().NotBe(0);
        loginResult.Data.Token.RefreshToken.Should().NotBeNull();
        loginResult.Data.Token.TokenType.Should().Be("Bearer");
        loginResult.Data.Token.Scope.Should().Be("Milvonion");
        loginResult.Data.Token.SessionState.Should().BeNull();
        loginResult.Data.AccessibleMenuItems.Should().BeNullOrEmpty();
        loginResult.Data.PageInformations.Should().BeNullOrEmpty();

        loginResult.IsSuccess.Should().BeTrue();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    #endregion

    #region Rate limit and token revocation

    [Fact]
    public async Task LoginAsync_OverTheRateLimit_ShouldBe429PerClientAddress()
    {
        // Arrange: the login limit is 10 requests per minute per client address, counted in a fixed window. Start in a
        // fresh window so the 11 requests below cannot straddle a window boundary.
        var secondsLeftInWindow = 60 - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 60);

        if (secondsLeftInWindow < 10)
            await Task.Delay(TimeSpan.FromSeconds(secondsLeftInWindow + 1), TestContext.Current.CancellationToken);

        var request = new LoginCommand
        {
            UserName = "admin",
            Password = "admin",
            DeviceId = "device-id"
        };

        var client = _factory.CreateClientWithAddress("203.0.113.10");
        client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var otherClient = _factory.CreateClientWithAddress("203.0.113.11");

        // Act
        for (var i = 0; i < 10; i++)
        {
            var (allowedResponse, _) = await client.PostAsync<LoginResponseDto>("account/login", request);

            allowedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var (limitedResponse, limitedResult) = await client.PostAsync<LoginResponseDto>("account/login", request);
        var (otherAddressResponse, _) = await otherClient.PostAsync<LoginResponseDto>("account/login", request);

        // Assert
        limitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        limitedResponse.Headers.RetryAfter.Should().NotBeNull();
        limitedResult.Should().NotBeNull();
        limitedResult.IsSuccess.Should().BeFalse();
        limitedResult.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
        limitedResult.Messages[0].Message.Should().Be("You're sending requests too quickly. Please slow down and try again.");

        otherAddressResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessToken_WithoutSessionStamp_ShouldBe401()
    {
        // Arrange: a correctly signed token for an existing user, but without the revocation stamp claim.
        using var scope = _factory.CreateScope();

        var tokenManager = scope.ServiceProvider.GetRequiredService<IMilvaTokenManager>();

        var accessToken = tokenManager.GenerateToken(DateTime.UtcNow.AddMinutes(30),
                                                     claims: [new Claim(ClaimTypes.Name, GlobalConstant.RootUsername), new Claim(GlobalConstant.UserIdClaimName, "1")]);

        var client = _factory.CreateClientWithAddress().UseAccessToken(accessToken);

        // Act
        var httpResponse = await client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DocumentationToken_ShouldCarryTheRootUsersCurrentStamp()
    {
        // Arrange
        var accessToken = await _factory.CreateClientWithAddress().GetDocumentationTokenAsync();

        var client = _factory.CreateClientWithAddress().UseAccessToken(accessToken);

        // Act
        var (httpResponse, result) = await client.GetAsync<AccountDetailDto>("account/detail");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task DocumentationToken_OnAFreshDatabase_ShouldReachOnlyTheBootstrapEndpoints()
    {
        // Arrange: a database nobody initialized yet, so there is no root user and the documentation token's stamp cannot be validated.
        var connectionString = await _factory.CreateEmptyDatabaseAsync();

        await using var freshHost = _factory.CreateHost(new() { [MilvonionApiFactory.ConnectionStringKey] = connectionString });

        var accessToken = await freshHost.CreateClientWithAddress().GetDocumentationTokenAsync();

        var client = freshHost.CreateClientWithAddress().UseAccessToken(accessToken);

        // Act
        var detailResponse = await client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);
        var initResponse = await client.PostAsync($"{TestApi.Base}/developer/database/init", null, TestContext.Current.CancellationToken);

        // Assert: the init endpoint runs (the test schema has no migration to initialize, so it reports an error) instead of rejecting the token.
        detailResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        initResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        initResponse.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    #endregion

    #region RefreshLoginAsync

    [Fact]
    public async Task RefreshLoginAsync_SecondUseOfTheSameRefreshToken_ShouldBeRejected()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();

        var request = new RefreshLoginCommand
        {
            UserName = user.UserName,
            DeviceId = user.DeviceId,
            RefreshToken = user.Token.RefreshToken
        };

        // Act
        var (firstResponse, firstResult) = await user.Client.PostAsync<LoginResponseDto>("account/login/refresh", request);
        var (replayResponse, _) = await user.Client.PostAsync<LoginResponseDto>("account/login/refresh", request);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResult.Data.Token.RefreshToken.Should().NotBe(request.RefreshToken);
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshLoginAsync_ConcurrentUseOfTheSameRefreshToken_ShouldSucceedOnlyOnce()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();

        var request = new RefreshLoginCommand
        {
            UserName = user.UserName,
            DeviceId = user.DeviceId,
            RefreshToken = user.Token.RefreshToken
        };

        // Act
        var responses = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => _factory.CreateClientWithAddress()
                                                                                    .UseAccessToken(user.Token.AccessToken)
                                                                                    .PostAsync<LoginResponseDto>("account/login/refresh", request)));

        // Assert
        responses.Count(r => r.Http.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(r => r.Http.StatusCode == HttpStatusCode.Unauthorized).Should().Be(1);
    }

    #endregion

    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_ShouldRevokeTheOldAccessToken()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();

        var request = new ChangePasswordCommand
        {
            UserName = user.UserName,
            OldPassword = user.Password,
            NewPassword = "N3w!pass"
        };

        // Act
        var (changeResponse, _) = await user.Client.PutAsync("account/password/change", request);
        var oldTokenResponse = await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        var newLogin = await _factory.LoginAsync(user.UserName, request.NewPassword);
        var newTokenResponse = await newLogin.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        // Assert
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        oldTokenResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        newTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithAPasswordAgainstThePolicy_ShouldBe400WithThePolicyMessage()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();

        user.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var request = new ChangePasswordCommand
        {
            UserName = user.UserName,
            OldPassword = user.Password,
            NewPassword = "Ab1!"
        };

        // Act
        var (httpResponse, result) = await user.Client.PutAsync("account/password/change", request);
        var detailResponse = await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        // Assert: rejected by the validator before anything changed, so the token still works.
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Messages[0].Message.Should().Be("Password lenght is too short.");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region AccountDetailsAsync

    [Fact]
    public async Task AccountDetailsAsync_WithLoggedInUser_ShouldReturnAccountDetails()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (httpResponse, loginResult) = await root.Client.GetAsync<AccountDetailDto>("account/detail");

        // Arrange
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResult.Should().NotBeNull();
        loginResult.Data.Should().NotBeNull();
        loginResult.Data.Id.Should().Be(root.Id);

        loginResult.IsSuccess.Should().BeTrue();
        loginResult.IsCachedData.Should().BeFalse();
        loginResult.Metadatas.Should().BeEmpty();
        loginResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task AccountDetailsAsync_WithAnotherUsersId_ShouldStillReturnTheCallersOwnAccount()
    {
        // Arrange
        var otherUser = await _factory.CreateUserAsync();
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (httpResponse, result) = await root.Client.GetAsync<AccountDetailDto>($"account/detail?UserId={otherUser.Id}");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Id.Should().Be(root.Id);
        result.Data.UserName.Should().Be(GlobalConstant.RootUsername);
    }

    #endregion

    /// <summary>
    /// Adds a user with a raw password hash (and lockout state) the way an older client or an import would have stored it.
    /// </summary>
    private async Task<string> AddUserAsync(string passwordHash, bool lockoutEnabled = false, int accessFailedCount = 0, DateTimeOffset? lockoutEnd = null)
    {
        var userName = $"login{TestApi.Unique()}";

        await _factory.DbAsync(async dbContext =>
        {
            await dbContext.Users.AddAsync(new User
            {
                UserName = userName,
                PasswordHash = passwordHash,
                LockoutEnabled = lockoutEnabled,
                AccessFailedCount = accessFailedCount,
                LockoutEnd = lockoutEnd
            }, TestContext.Current.CancellationToken);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        });

        return userName;
    }
}
