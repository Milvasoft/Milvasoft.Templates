using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Types.Structs;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Dtos.UserDtos;
using Milvonion.Application.Features.Account.RefreshLogin;
using Milvonion.Application.Features.Users.CreateUser;
using Milvonion.Application.Features.Users.GetUserList;
using Milvonion.Application.Features.Users.UpdateUser;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.IntegrationTests.TestBase;
using System.Net;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the user management endpoints.")]
public class UsersControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    #region AddUserAsync

    [Fact]
    public async Task AddUserAsync_WithAnEmailInUse_ShouldBe409()
    {
        // Arrange
        var email = $"taken{TestApi.Unique()}@example.com";

        await _factory.CreateUserAsync(email: email);

        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        // Same address in another casing: the e-mail is compared normalized.
        var request = NewUser($"second{TestApi.Unique()}", email.ToUpperInvariant());

        // Act
        var (httpResponse, result) = await root.Client.PostAsync<int>("users/user", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        result.Messages[0].Message.Should().Be("There is a user with the same email!");
    }

    [Fact]
    public async Task AddUserAsync_WithoutEmail_ShouldAllowManyUsers()
    {
        // Arrange: the e-mail is optional, so only a set e-mail is unique.
        var root = await _factory.LoginAsRootAsync();

        // Act
        var responses = new List<HttpResponseMessage>
        {
            (await root.Client.PostAsync<int>("users/user", NewUser($"noemail{TestApi.Unique()}", null))).Http,
            (await root.Client.PostAsync<int>("users/user", NewUser($"noemail{TestApi.Unique()}", null))).Http,
            (await root.Client.PostAsync<int>("users/user", NewUser($"emptyemail{TestApi.Unique()}", string.Empty))).Http,
            (await root.Client.PostAsync<int>("users/user", NewUser($"emptyemail{TestApi.Unique()}", string.Empty))).Http
        };

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddUserAsync_WithAPasswordAgainstThePolicy_ShouldBe400WithThePolicyMessage()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var request = NewUser($"weak{TestApi.Unique()}", null);
        request.Password = "abcdefgh";

        // Act
        var (httpResponse, result) = await root.Client.PostAsync<int>("users/user", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Messages[0].Message.Should().Be("Password must have at least one alphanumeric character.");
    }

    [Fact]
    public async Task AddUserAsync_WithNamesAtTheColumnLimit_ShouldSucceedAndOneCharacterMoreShouldBe400()
    {
        // Arrange: create and update share the column length of the entity.
        var root = await _factory.LoginAsRootAsync();

        var atLimit = NewUser($"limit{TestApi.Unique()}", null);
        atLimit.Name = new string('n', User.NameMaxLength);
        atLimit.Surname = new string('s', User.NameMaxLength);

        var overLimit = NewUser($"overlimit{TestApi.Unique()}", null);
        overLimit.Surname = new string('s', User.NameMaxLength + 1);

        // Act
        var (atLimitResponse, _) = await root.Client.PostAsync<int>("users/user", atLimit);
        var (overLimitResponse, overLimitResult) = await root.Client.PostAsync<int>("users/user", overLimit);

        // Assert
        atLimitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        overLimitResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        overLimitResult.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetUsersAsync / GetUserAsync

    [Fact]
    public async Task GetUsersAndGetUserAsync_ShouldReturnTheUserTypeAndTheLockState()
    {
        // Arrange: a locked manager and an unlocked app user.
        var locked = await _factory.CreateUserAsync(userType: UserType.Manager);
        var unlocked = await _factory.CreateUserAsync();
        var root = await _factory.LoginAsRootAsync();

        var (lockResponse, _) = await root.Client.PutAsync<int>("users/user", new UpdateUserCommand { Id = locked.Id, Lockout = new UpdateProperty<bool>(true) });

        var listQuery = new GetUserListQuery
        {
            RowCount = 10,
            Filtering = new FilterRequest
            {
                Criterias = [new FilterCriteria { FilterBy = nameof(UserListDto.UserName), Type = FilterType.In, Value = new[] { locked.UserName, unlocked.UserName } }]
            }
        };

        // Act
        var (listResponse, list) = await root.Client.PatchListAsync<UserListDto>("users", listQuery);
        var (lockedDetailResponse, lockedDetail) = await root.Client.GetAsync<UserStateDto>($"users/user?UserId={locked.Id}");
        var (unlockedDetailResponse, unlockedDetail) = await root.Client.GetAsync<UserStateDto>($"users/user?UserId={unlocked.Id}");

        // Assert
        lockResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        lockedDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        unlockedDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var lockedItem = list.Data.Single(u => u.Id == locked.Id);
        var unlockedItem = list.Data.Single(u => u.Id == unlocked.Id);

        lockedItem.UserType.Should().Be(UserType.Manager);
        lockedItem.IsLocked.Should().BeTrue();
        lockedItem.LockoutEnd.Should().BeAfter(DateTimeOffset.UtcNow);
        unlockedItem.UserType.Should().Be(UserType.AppUser);
        unlockedItem.IsLocked.Should().BeFalse();
        unlockedItem.LockoutEnd.Should().BeNull();

        lockedDetail.Data.UserType.Should().Be(UserType.Manager);
        lockedDetail.Data.IsLocked.Should().BeTrue();
        lockedDetail.Data.LockoutEnd.Should().Be(lockedItem.LockoutEnd);
        unlockedDetail.Data.UserType.Should().Be(UserType.AppUser);
        unlockedDetail.Data.IsLocked.Should().BeFalse();
        unlockedDetail.Data.LockoutEnd.Should().BeNull();
    }

    #endregion

    #region UpdateUserAsync / RemoveUserAsync

    [Fact]
    public async Task UpdateUserAsync_WithLockout_ShouldRevokeTheUsersTokens()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();
        var root = await _factory.LoginAsRootAsync();

        var beforeResponse = await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        var request = new UpdateUserCommand
        {
            Id = user.Id,
            Lockout = new UpdateProperty<bool>(true)
        };

        // Act
        var (updateResponse, _) = await root.Client.PutAsync<int>("users/user", request);
        var afterResponse = await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);
        var (refreshResponse, _) = await user.Client.PostAsync<LoginResponseDto>("account/login/refresh", new RefreshLoginCommand
        {
            UserName = user.UserName,
            DeviceId = user.DeviceId,
            RefreshToken = user.Token.RefreshToken
        });

        // Assert
        beforeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        afterResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var sessionCount = await _factory.DbAsync(db => db.UserSessions.CountAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken));
        var archivedSessionCount = await _factory.DbAsync(db => db.UserSessionHistories.CountAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken));

        sessionCount.Should().Be(0);
        archivedSessionCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateUserAsync_WithoutSecurityRelevantChanges_ShouldKeepTheUsersTokens()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();
        var root = await _factory.LoginAsRootAsync();

        var request = new UpdateUserCommand
        {
            Id = user.Id,
            Name = new UpdateProperty<string>("Renamed")
        };

        // Act
        var (updateResponse, _) = await root.Client.PutAsync<int>("users/user", request);
        var (detailResponse, detail) = await user.Client.GetAsync<AccountDetailDto>("account/detail");

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        detail.Data.Name.Should().Be("Renamed");
    }

    [Fact]
    public async Task UpdateUserAsync_WithANameOverTheColumnLimit_ShouldBe400AndChangeNothing()
    {
        // Arrange
        var user = await _factory.CreateUserAsync();
        var root = await _factory.LoginAsRootAsync();

        var request = new UpdateUserCommand
        {
            Id = user.Id,
            Name = new UpdateProperty<string>(new string('n', User.NameMaxLength + 1))
        };

        // Act
        var (httpResponse, result) = await root.Client.PutAsync<int>("users/user", request);
        var (atLimitResponse, _) = await root.Client.PutAsync<int>("users/user", new UpdateUserCommand { Id = user.Id, Surname = new UpdateProperty<string>(new string('s', User.NameMaxLength)) });

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
        atLimitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var row = await _factory.GetUserRowAsync(user.UserName);

        row.Name.Should().Be(user.Name);
        row.Surname.Should().Be(new string('s', User.NameMaxLength));
    }

    [Fact]
    public async Task UpdateUserAsync_WithAnIdNobodyHas_ShouldBe404AndChangeNothing()
    {
        // Arrange: the id is above every id the run creates, and the request also asks for a role change and a lockout.
        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var request = new UpdateUserCommand
        {
            Id = int.MaxValue,
            Name = new UpdateProperty<string>("Nobody"),
            Lockout = new UpdateProperty<bool>(true),
            RoleIdList = new UpdateProperty<List<int>>([1])
        };

        // Act
        var (httpResponse, result) = await root.Client.PutAsync<int>("users/user", request);

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        result.Messages[0].Message.Should().Be("User not found!");

        var roleRelationCount = await _factory.DbAsync(db => db.UserRoleRelations.CountAsync(r => r.UserId == int.MaxValue, TestContext.Current.CancellationToken));

        roleRelationCount.Should().Be(0);
    }

    [Fact]
    public async Task RemoveUserAsync_ShouldRevokeTheUsersAccessToken()
    {
        // Arrange
        var user = await _factory.CreateAndLoginUserAsync();
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (deleteResponse, _) = await root.Client.DeleteAsync<int>($"users/user?UserId={user.Id}");
        var afterResponse = await user.Client.GetAsync($"{TestApi.Base}/account/detail", TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        afterResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    /// <summary>
    /// The fields of <see cref="UserDetailDto"/> the lock state test reads (its AuditDto cannot be deserialized).
    /// </summary>
    private sealed record UserStateDto(int Id, UserType UserType, DateTimeOffset? LockoutEnd, bool IsLocked);

    private static CreateUserCommand NewUser(string userName, string email) => new()
    {
        UserName = userName,
        Email = email,
        Name = "Test",
        Surname = "User",
        Password = "Str0ng!pass",
        UserType = UserType.AppUser,
        RoleIdList = [1]
    };
}
