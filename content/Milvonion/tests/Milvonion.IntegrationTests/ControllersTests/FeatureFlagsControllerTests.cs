using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Types.Structs;
using Milvonion.Application.Dtos.ExportDtos;
using Milvonion.Application.Dtos.FeatureFlagDtos;
using Milvonion.Application.Features.FeatureFlags.UpdateFeatureFlag;
using Milvonion.Domain;
using Milvonion.Domain.Enums;
using Milvonion.IntegrationTests.TestBase;
using System.Net;
using System.Net.Http.Json;

namespace Milvonion.IntegrationTests.ControllersTests;

[Collection(nameof(MilvonionApiCollection))]
[Trait("Controller Integration Tests", "Integration tests for the feature flag (kill switch) endpoints and the FeatureFlag attribute.")]
public class FeatureFlagsControllerTests(MilvonionApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly ExportRequest _userExport = new()
    {
        ExportType = ExportType.User,
        ListRequest = new ListRequest { PageNumber = 1, RowCount = 10 }
    };

    [Fact]
    public async Task GetFeatureFlagsAsync_ShouldListEveryKnownSwitchEnabledByDefault()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (httpResponse, result) = await root.Client.PatchListAsync<FeatureFlagListDto>("featureFlags");

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Select(f => f.Key).Should().Contain(FeatureFlagKeys.All);
        result.Data.Single(f => f.Key == FeatureFlagKeys.Export).IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFeatureFlagAsync_SwitchingExportOff_ShouldAnswer503AtOnceUntilSwitchedOnAgain()
    {
        // Arrange: an export first, so the switch states are cached before the toggle.
        var root = await _factory.LoginAsRootAsync();

        root.Client.DefaultRequestHeaders.Add("Accept-Language", "en-US");

        var exportFlagId = await GetFlagIdAsync(root, FeatureFlagKeys.Export);

        var enabledExport = await ExportAsync(root);

        try
        {
            // Act
            var (switchOffResponse, _) = await root.Client.PutAsync<int>("featureFlags/featureFlag", new UpdateFeatureFlagCommand
            {
                Id = exportFlagId,
                IsEnabled = new UpdateProperty<bool>(false),
                Reason = new UpdateProperty<string>("Maintenance")
            });

            var disabledResponse = await ExportAsync(root);
            var disabledResult = await TestApi.ReadAsync<Response>(disabledResponse);

            var (switchOnResponse, _) = await root.Client.PutAsync<int>("featureFlags/featureFlag", new UpdateFeatureFlagCommand
            {
                Id = exportFlagId,
                IsEnabled = new UpdateProperty<bool>(true),
                Reason = new UpdateProperty<string>(null)
            });

            var reenabledExport = await ExportAsync(root);

            // Assert
            enabledExport.StatusCode.Should().Be(HttpStatusCode.OK);
            switchOffResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            disabledResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable, "the cached state is dropped by the toggle, so the switch applies to the very next request");
            disabledResult.IsSuccess.Should().BeFalse();
            disabledResult.StatusCode.Should().Be((int)HttpStatusCode.ServiceUnavailable);
            disabledResult.Messages[0].Message.Should().Be("This feature is currently unavailable. Please try again later.");

            switchOnResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            reenabledExport.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            // The database is shared: never leave the switch off for the next test.
            await _factory.DbAsync(db => db.FeatureFlags.Where(f => f.Id == exportFlagId).ExecuteUpdateAsync(s => s.SetProperty(f => f.IsEnabled, true)));
        }
    }

    [Fact]
    public async Task FeatureFlagEndpoints_WithoutThePermissions_ShouldBe403AndChangeNothing()
    {
        // Arrange: a manager (the controller's user type) without any role.
        var manager = await _factory.CreateAndLoginUserAsync(UserType.Manager);
        var root = await _factory.LoginAsRootAsync();

        var exportFlagId = await GetFlagIdAsync(root, FeatureFlagKeys.Export);

        // Act
        var (listResponse, _) = await manager.Client.PatchListAsync<FeatureFlagListDto>("featureFlags");
        var (updateResponse, _) = await manager.Client.PutAsync<int>("featureFlags/featureFlag", new UpdateFeatureFlagCommand
        {
            Id = exportFlagId,
            IsEnabled = new UpdateProperty<bool>(false)
        });

        // Assert
        listResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var (_, flags) = await root.Client.PatchListAsync<FeatureFlagListDto>("featureFlags");

        flags.Data.Single(f => f.Key == FeatureFlagKeys.Export).IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFeatureFlagAsync_WithAnIdNobodyHas_ShouldBe404()
    {
        // Arrange
        var root = await _factory.LoginAsRootAsync();

        // Act
        var (httpResponse, result) = await root.Client.PutAsync<int>("featureFlags/featureFlag", new UpdateFeatureFlagCommand
        {
            Id = int.MaxValue,
            IsEnabled = new UpdateProperty<bool>(false)
        });

        // Assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Posts the user export. A successful export is a file, not an envelope, so the raw response is returned.
    /// </summary>
    private static Task<HttpResponseMessage> ExportAsync(TestUser admin) => admin.Client.PostAsJsonAsync($"{TestApi.Base}/exports/export", _userExport, TestApi.Json);

    private static async Task<int> GetFlagIdAsync(TestUser admin, string key)
    {
        var (_, flags) = await admin.Client.PatchListAsync<FeatureFlagListDto>("featureFlags");

        return flags.Data.Single(f => f.Key == key).Id;
    }
}
