using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvonion.Api.AppStartup;
using Milvonion.Api.Utils;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.Infrastructure.Persistence.Context;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Milvonion.IntegrationTests.TestBase;

/// <summary>
/// A user logged in through the real login endpoint, with an authenticated client.
/// </summary>
public sealed class TestUser
{
    /// <summary>User id.</summary>
    public int Id { get; init; }

    /// <summary>User name.</summary>
    public string UserName { get; init; }

    /// <summary>Password the user logs in with.</summary>
    public string Password { get; init; }

    /// <summary>Device the session belongs to.</summary>
    public string DeviceId { get; init; }

    /// <summary>Tokens of the login.</summary>
    public MilvaToken Token { get; init; }

    /// <summary>Client sending <see cref="Token"/> as its bearer token, with its own forwarded address.</summary>
    public HttpClient Client { get; init; }
}

/// <summary>
/// HTTP and database helpers shared by the integration tests. Everything a client can do goes through the real
/// endpoints; the database is touched directly only to read state back or to create what no endpoint creates
/// (a user with a known password and no permissions).
/// </summary>
public static class TestApi
{
    /// <summary>Route prefix of every versioned endpoint.</summary>
    public const string Base = $"{GlobalConstant.RoutePrefix}/v1.0";

    /// <summary>Password of the root user the factory seeds (<see cref="GlobalConstant.RootUsername"/>).</summary>
    public const string RootPassword = "defaultpass";

    /// <summary>Password of the users the tests create; satisfies the configured password policy.</summary>
    public const string DefaultPassword = "Test1234!";

    /// <summary>Serializer options of the API (camelCase).</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static int _sequence;
    private static int _clientSequence;

    /// <summary>Unique, user name rule compliant suffix (starts with a letter where the caller adds none).</summary>
    public static string Unique() => $"{DateTime.UtcNow:HHmmss}{Interlocked.Increment(ref _sequence)}{Random.Shared.Next(100, 999)}";

    /// <summary>
    /// A client with its own (forwarded) source address. The API runs behind a proxy and trusts the X-Forwarded-For
    /// hop it appends, so each client here is a distinct caller for the per-IP rate limits (and the same client is
    /// the same caller, which is what the rate limit tests rely on). Works on the shared factory and on a host created
    /// by <see cref="MilvonionApiFactory.CreateHost"/>.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    public static HttpClient CreateClientWithAddress(this WebApplicationFactory<IApiMarker> host, string address = null)
    {
        var client = host.CreateClient();

        var sequence = Interlocked.Increment(ref _clientSequence);

        client.DefaultRequestHeaders.Add("X-Forwarded-For", address ?? $"10.{(sequence >> 16) & 255}.{(sequence >> 8) & 255}.{sequence & 255}");

        return client;
    }

    /// <summary>
    /// Creates a user directly in the database (an app user with no role unless <paramref name="roleIds"/> are given)
    /// that can log in with <see cref="DefaultPassword"/>. The id comes from the database, so it never collides with
    /// another test's user.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="userName"></param>
    /// <param name="email"></param>
    /// <param name="userType"></param>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    public static async Task<User> CreateUserAsync(this MilvonionApiFactory factory, string userName = null, string email = null, UserType userType = UserType.AppUser, params int[] roleIds)
    {
        userName ??= $"user{Unique()}";

        using var scope = factory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MilvonionDbContext>();

        var user = new User
        {
            UserName = userName,
            NormalizedUserName = userName.MilvaNormalize(),
            Email = email,
            NormalizedEmail = email?.MilvaNormalize(),
            Name = "Test",
            Surname = "User",
            UserType = userType,
            CreationDate = DateTime.UtcNow,
            CreatorUserName = GlobalConstant.SystemUsername,
            LockoutEnabled = true,
            RoleRelations = [.. roleIds.Select(roleId => new UserRoleRelation { RoleId = roleId })]
        };

        scope.ServiceProvider.GetRequiredService<IMilvaUserManager<User, int>>().SetPasswordHash(user, DefaultPassword);

        await dbContext.Users.AddAsync(user);

        await dbContext.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Creates a user (see <see cref="CreateUserAsync"/>) and logs it in.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="userType"></param>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    public static async Task<TestUser> CreateAndLoginUserAsync(this MilvonionApiFactory factory, UserType userType = UserType.AppUser, params int[] roleIds)
    {
        var user = await factory.CreateUserAsync(userType: userType, roleIds: roleIds);

        return await factory.LoginAsync(user.UserName, DefaultPassword);
    }

    /// <summary>
    /// Logs the seeded root user (SuperAdmin) in. The root user is shared by every test: use it for reads and for
    /// admin actions on other users, never change it (its row is protected by the seed triggers anyway).
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Task<TestUser> LoginAsRootAsync(this MilvonionApiFactory factory) => factory.LoginAsync(GlobalConstant.RootUsername, RootPassword);

    /// <summary>
    /// Logs in through the real endpoint and returns an authenticated client. Throws when the login is rejected.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public static async Task<TestUser> LoginAsync(this WebApplicationFactory<IApiMarker> host, string userName, string password, string deviceId = null)
    {
        deviceId ??= $"device-{Unique()}";

        var client = host.CreateClientWithAddress();

        var (http, body) = await client.RequestLoginAsync(userName, password, deviceId);

        if (body is null || !body.IsSuccess || body.Data?.Token?.AccessToken is null)
            throw new InvalidOperationException($"Login failed ({(int)http.StatusCode}): {body?.MessagesToString()}");

        client.UseAccessToken(body.Data.Token.AccessToken);

        return new TestUser { Id = body.Data.Id, UserName = userName, Password = password, DeviceId = deviceId, Token = body.Data.Token, Client = client };
    }

    /// <summary>
    /// Posts a login request and returns the response with its envelope (tokens included) without touching the client's headers.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public static Task<(HttpResponseMessage Http, Response<LoginResponseDto> Body)> RequestLoginAsync(this HttpClient client, string userName, string password, string deviceId = "device-id")
        => client.PostAsync<LoginResponseDto>("account/login", new LoginCommand { UserName = userName, Password = password, DeviceId = deviceId });

    /// <summary>
    /// Sends <paramref name="accessToken"/> as the bearer token of every following request of the client.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static HttpClient UseAccessToken(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }

    /// <summary>
    /// Reads the bearer token the API documentation (Scalar) page hands out: the root user's, with its current stamp.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public static async Task<string> GetDocumentationTokenAsync(this HttpClient client)
    {
        var html = await client.GetStringAsync($"{GlobalConstant.RoutePrefix}/documentation", TestContext.Current.CancellationToken);

        var match = Regex.Match(html, "\"token\"\\s*:\\s*\"([^\"]+)\"");

        if (!match.Success)
            throw new InvalidOperationException("The documentation page carries no bearer token.");

        return match.Groups[1].Value;
    }

    /// <summary>
    /// Runs <paramref name="work"/> against a fresh DbContext scope.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    /// <param name="work"></param>
    /// <returns></returns>
    public static async Task<T> DbAsync<T>(this MilvonionApiFactory factory, Func<MilvonionDbContext, Task<T>> work)
    {
        using var scope = factory.CreateScope();

        return await work(scope.ServiceProvider.GetRequiredService<MilvonionDbContext>());
    }

    /// <summary>
    /// Runs <paramref name="work"/> against a fresh DbContext scope.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="work"></param>
    /// <returns></returns>
    public static async Task DbAsync(this MilvonionApiFactory factory, Func<MilvonionDbContext, Task> work)
    {
        using var scope = factory.CreateScope();

        await work(scope.ServiceProvider.GetRequiredService<MilvonionDbContext>());
    }

    /// <summary>
    /// Resolves a scoped service and runs <paramref name="work"/> with it.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="factory"></param>
    /// <param name="work"></param>
    /// <returns></returns>
    public static async Task<T> WithServiceAsync<TService, T>(this MilvonionApiFactory factory, Func<TService, Task<T>> work) where TService : notnull
    {
        using var scope = factory.CreateScope();

        return await work(scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>
    /// Reads a user row back from the database.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public static Task<User> GetUserRowAsync(this MilvonionApiFactory factory, string userName)
        => factory.DbAsync(db => db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == userName));

    #region Envelope aware HTTP helpers (paths are relative to Base)

    /// <summary>POSTs <paramref name="body"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response<T> Body)> PostAsync<T>(this HttpClient client, string path, object body)
    {
        var http = await client.PostAsJsonAsync($"{Base}/{path}", body, Json);

        return (http, await ReadAsync<Response<T>>(http));
    }

    /// <summary>POSTs <paramref name="body"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response Body)> PostAsync(this HttpClient client, string path, object body)
    {
        var http = await client.PostAsJsonAsync($"{Base}/{path}", body, Json);

        return (http, await ReadAsync<Response>(http));
    }

    /// <summary>POSTs with an <c>Idempotency-Key</c> header, as a client retrying one user action does.</summary>
    public static async Task<(HttpResponseMessage Http, Response<T> Body)> PostWithKeyAsync<T>(this HttpClient client, string path, object body, string idempotencyKey)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{Base}/{path}")
        {
            Content = JsonContent.Create(body, options: Json)
        };

        request.Headers.TryAddWithoutValidation(IdempotentAttribute.HeaderName, idempotencyKey);

        var http = await client.SendAsync(request);

        return (http, await ReadAsync<Response<T>>(http));
    }

    /// <summary>PUTs <paramref name="body"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response<T> Body)> PutAsync<T>(this HttpClient client, string path, object body)
    {
        var http = await client.PutAsJsonAsync($"{Base}/{path}", body, Json);

        return (http, await ReadAsync<Response<T>>(http));
    }

    /// <summary>PUTs <paramref name="body"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response Body)> PutAsync(this HttpClient client, string path, object body)
    {
        var http = await client.PutAsJsonAsync($"{Base}/{path}", body, Json);

        return (http, await ReadAsync<Response>(http));
    }

    /// <summary>PATCHes <paramref name="body"/> (the list endpoints) and reads the list envelope.</summary>
    public static async Task<(HttpResponseMessage Http, ListResponse<T> Body)> PatchListAsync<T>(this HttpClient client, string path, object body = null)
    {
        var http = await client.PatchAsJsonAsync($"{Base}/{path}", body ?? new { pageNumber = 1, rowCount = 50 }, Json);

        return (http, await ReadAsync<ListResponse<T>>(http));
    }

    /// <summary>GETs <paramref name="path"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response<T> Body)> GetAsync<T>(this HttpClient client, string path)
    {
        var http = await client.GetAsync($"{Base}/{path}");

        return (http, await ReadAsync<Response<T>>(http));
    }

    /// <summary>DELETEs <paramref name="path"/> and reads the envelope.</summary>
    public static async Task<(HttpResponseMessage Http, Response<T> Body)> DeleteAsync<T>(this HttpClient client, string path)
    {
        var http = await client.DeleteAsync($"{Base}/{path}");

        return (http, await ReadAsync<Response<T>>(http));
    }

    /// <summary>Reads the body as <typeparamref name="T"/>; null for an empty body.</summary>
    public static async Task<T> ReadAsync<T>(HttpResponseMessage http) where T : class
    {
        var text = await http.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
            return null;

        return JsonSerializer.Deserialize<T>(text, Json);
    }

    #endregion
}
