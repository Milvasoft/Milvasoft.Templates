using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Application.Dtos.AccountDtos;
using Milvonion.Application.Features.Account.Login;
using Milvonion.Application.Utils.Constants;
using System.Net.Http.Json;

namespace Milvonion.IntegrationTests.TestBase;
public static class TestExtensions
{
    public static async Task<HttpClient> LoginAsync(this HttpClient client, string username = "rootuser", string password = "defaultpass", string deviceId = "device-id")
    {
        var request = new LoginCommand
        {
            UserName = username,
            Password = password,
            DeviceId = deviceId
        };

        var httpResponse = await client.PostAsJsonAsync($"{GlobalConstant.RoutePrefix}/v1.0/account/login", request);
        var loginResult = await httpResponse.Content.ReadFromJsonAsync<Response<LoginResponseDto>>();

        if (loginResult is not null || loginResult.IsSuccess)
            client.DefaultRequestHeaders.Add("Authorization", $"{loginResult.Data.Token.TokenType} {loginResult.Data.Token.AccessToken}");

        return client;
    }
}
