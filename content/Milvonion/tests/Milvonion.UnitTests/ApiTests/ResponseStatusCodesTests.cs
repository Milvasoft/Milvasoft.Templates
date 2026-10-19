using FluentAssertions;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="ResponseStatusCodes"/>: the single mapping from failure message keys to wire status codes.
/// </summary>
[Trait("Api Unit Tests", "Response status code mapping.")]
public class ResponseStatusCodesTests
{
    [Theory]
    [InlineData(MessageKey.Unauthorized, 401)]
    [InlineData(MessageKey.SessionTimeout, 401)]
    [InlineData(MessageKey.Forbidden, 403)]
    [InlineData(MessageKey.Locked, 403)]
    [InlineData(MessageKey.LockedIndefinite, 403)]
    [InlineData(MessageKey.UserNotFound, 404)]
    [InlineData(MessageKey.RoleNotFound, 404)]
    [InlineData(MessageKey.PageNotFound, 404)]
    [InlineData(MessageKey.NamespaceNotFound, 404)]
    [InlineData(MessageKey.ResourceGroupNotFound, 404)]
    [InlineData(MessageKey.ContentNotFound, 404)]
    [InlineData(MessageKey.MediaNotFound, 404)]
    [InlineData(MessageKey.InternalNotificationNotFound, 404)]
    [InlineData(MessageKey.FeatureFlagNotFound, 404)]
    [InlineData(MessageKey.UserExists, 409)]
    [InlineData(MessageKey.EmailExists, 409)]
    [InlineData(MessageKey.PostgreDuplicateDataException, 409)]
    [InlineData(MessageKey.DefaultValueCannotModify, 409)]
    [InlineData(MessageKey.IdempotencyKeyReused, 409)]
    [InlineData(MessageKey.RequestInProgress, 409)]
    [InlineData(MessageKey.TooManyRequests, 429)]
    [InlineData(MessageKey.UnhandledException, 500)]
    [InlineData(MessageKey.PostgreBasedException, 500)]
    [InlineData(MessageKey.FeatureDisabled, 503)]
    public void FromKey_ShouldMapKnownKeysToTheirStatus(string key, int expected)
    {
        ResponseStatusCodes.FromKey(key).Should().Be(expected);
    }

    [Theory]
    [InlineData("SomethingNobodyMapped")]
    [InlineData("")]
    [InlineData(null)]
    public void FromKey_ShouldFallBackTo400_ForUnknownOrEmptyKeys(string key)
    {
        ResponseStatusCodes.FromKey(key).Should().Be(400);
    }

    [Fact]
    public void FromKey_ShouldBeCaseSensitive()
    {
        // The dictionary is ordinal: a differently-cased key is not a known key and must not leak a 4xx/5xx.
        ResponseStatusCodes.FromKey(MessageKey.UserNotFound.ToUpperInvariant()).Should().Be(400);
    }

    [Fact]
    public void FromResponse_ShouldReturn200_ForSuccessfulResponseEvenWithErrorLookingMessages()
    {
        var response = Response.Success(MessageKey.UnhandledException);
        response.AddMessage(MessageKey.UserNotFound);

        ResponseStatusCodes.FromResponse(response).Should().Be(200);
    }

    [Fact]
    public void FromResponse_ShouldReturn200_ForNullResponse()
    {
        ResponseStatusCodes.FromResponse(null).Should().Be(200);
    }

    [Fact]
    public void FromResponse_ShouldReturn400_ForFailedResponseWithNullMessages()
    {
        var response = Response.Error();
        response.Messages = null;

        ResponseStatusCodes.FromResponse(response).Should().Be(400);
    }

    [Fact]
    public void FromResponse_ShouldReturn400_ForFailedResponseWithOnlyUnknownMessages()
    {
        var response = Response.Error("Title is required");
        response.AddMessage("Another validation message");

        ResponseStatusCodes.FromResponse(response).Should().Be(400);
    }

    [Fact]
    public void FromResponse_ShouldUseFirstNon400Message()
    {
        var response = Response.Error("validation text");
        response.AddMessage(MessageKey.UserNotFound);
        response.AddMessage(MessageKey.UnhandledException);

        ResponseStatusCodes.FromResponse(response).Should().Be(404);
    }

    [Fact]
    public void FromResponse_ShouldPreferKeyOverMessage_WhenKeyIsSet()
    {
        // After translation the Message holds human text and Key holds the message key.
        var response = Response.Error(new ResponseMessage(MessageKey.Forbidden, "You cannot do that", MessageType.Warning));

        ResponseStatusCodes.FromResponse(response).Should().Be(403);
    }

    [Fact]
    public void FromResponse_ShouldFallBackToMessage_WhenKeyIsEmpty()
    {
        var response = Response.Error(new ResponseMessage(string.Empty, MessageKey.UserExists, MessageType.Warning));

        ResponseStatusCodes.FromResponse(response).Should().Be(409);
    }

    [Fact]
    public void FromResponse_ShouldIgnoreNullMessageAndKey()
    {
        var response = Response.Error(new ResponseMessage(null, null, MessageType.Error));

        ResponseStatusCodes.FromResponse(response).Should().Be(400);
    }
}
