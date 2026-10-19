using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvonion.Api.Utils;
using Milvonion.Application.Utils.Constants;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="ResponseStatusCodeFilter"/>: failed envelopes leave the pipeline with a real HTTP status.
/// </summary>
[Trait("Api Unit Tests", "Response status code result filter.")]
public class ResponseStatusCodeFilterTests
{
    private static ResultExecutingContext Context(IActionResult result)
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

        return new ResultExecutingContext(actionContext, [], result, controller: new object());
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldSet404OnBothObjectResultAndEnvelope_ForNotFoundFailure()
    {
        // Arrange
        var response = Response.Error(MessageKey.UserNotFound);
        var objectResult = new ObjectResult(response);
        var context = Context(objectResult);
        var nextCalls = 0;

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => { nextCalls++; return Task.FromResult<ResultExecutedContext>(null); });

        // Assert
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldLeavePreset429Untouched()
    {
        // Arrange: the rate limiter already decided the status; the envelope carries a key that maps to 429 too,
        // but even if it did not, a preset non-200 status must never be overwritten.
        var response = Response.Error(MessageKey.UserNotFound);
        response.StatusCode = StatusCodes.Status429TooManyRequests;
        var objectResult = new ObjectResult(response) { StatusCode = StatusCodes.Status429TooManyRequests };
        var context = Context(objectResult);
        var nextCalls = 0;

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => { nextCalls++; return Task.FromResult<ResultExecutedContext>(null); });

        // Assert
        objectResult.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldRemapAnExplicit200_WhenEnvelopeFailed()
    {
        // Arrange: controllers return Ok(response) which pins StatusCode = 200; that must still be corrected.
        var response = Response.Error(MessageKey.Forbidden);
        var objectResult = new OkObjectResult(response);
        var context = Context(objectResult);

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => Task.FromResult<ResultExecutedContext>(null));

        // Assert
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldLeaveSuccessfulEnvelopeUntouched()
    {
        // Arrange
        var response = Response.Success();
        var originalStatus = response.StatusCode;
        var objectResult = new ObjectResult(response);
        var context = Context(objectResult);

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => Task.FromResult<ResultExecutedContext>(null));

        // Assert
        objectResult.StatusCode.Should().BeNull();
        response.StatusCode.Should().Be(originalStatus);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldUse400_ForFailureWithUnknownKey()
    {
        // Arrange
        var response = Response.Error("Title must not be empty");
        var objectResult = new ObjectResult(response);
        var context = Context(objectResult);

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => Task.FromResult<ResultExecutedContext>(null));

        // Assert
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldIgnoreNonResponseValues()
    {
        // Arrange
        var objectResult = new ObjectResult(new { Message = MessageKey.UserNotFound });
        var context = Context(objectResult);
        var nextCalls = 0;

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => { nextCalls++; return Task.FromResult<ResultExecutedContext>(null); });

        // Assert
        objectResult.StatusCode.Should().BeNull();
        nextCalls.Should().Be(1);
    }

    [Fact]
    public async Task OnResultExecutionAsync_ShouldIgnoreNonObjectResults()
    {
        // Arrange
        var context = Context(new StatusCodeResult(StatusCodes.Status204NoContent));
        var nextCalls = 0;

        // Act
        await new ResponseStatusCodeFilter().OnResultExecutionAsync(context, () => { nextCalls++; return Task.FromResult<ResultExecutedContext>(null); });

        // Assert
        context.Result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        nextCalls.Should().Be(1);
    }
}
