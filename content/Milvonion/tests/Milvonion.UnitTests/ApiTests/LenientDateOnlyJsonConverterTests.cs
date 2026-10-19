using FluentAssertions;
using Milvonion.Api.Utils;
using System.Text.Json;

namespace Milvonion.UnitTests.ApiTests;

/// <summary>
/// <see cref="LenientDateOnlyJsonConverter"/>: a date of birth is read by the date it was written with, whatever time
/// or offset follows it, and is written back as "yyyy-MM-dd".
/// </summary>
[Trait("Api Unit Tests", "Date-only JSON.")]
public class LenientDateOnlyJsonConverterTests
{
    private static readonly JsonSerializerOptions _options = new() { Converters = { new LenientDateOnlyJsonConverter() } };

    private sealed record Body(DateOnly? BirthDate);

    [Theory]
    [InlineData("1992-01-01")]
    [InlineData("1992-01-01T00:00:00Z")]
    [InlineData("1992-01-01T00:00:00.000Z")]
    [InlineData("1992-01-01T00:00:00+03:00")]
    [InlineData("1992-01-01T23:30:00-05:00")]
    [InlineData("1992-01-01 00:00:00")]
    public void Read_ShouldTakeTheDateAsWritten(string value)
    {
        JsonSerializer.Deserialize<Body>($$"""{"BirthDate":"{{value}}"}""", _options).BirthDate.Should().Be(new DateOnly(1992, 1, 1));
    }

    [Fact]
    public void Read_ShouldLeaveNullNull()
    {
        JsonSerializer.Deserialize<Body>("""{"BirthDate":null}""", _options).BirthDate.Should().BeNull();
    }

    [Theory]
    [InlineData("\"01.01.1992\"")]
    [InlineData("\"1992-13-01\"")]
    [InlineData("\"1992-01-01X\"")]
    [InlineData("\"\"")]
    [InlineData("19920101")]
    public void Read_ShouldRefuseWhatIsNotADate(string json)
    {
        var read = () => JsonSerializer.Deserialize<Body>($$"""{"BirthDate":{{json}}}""", _options);

        read.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ShouldWriteTheDateOnly()
    {
        JsonSerializer.Serialize(new Body(new DateOnly(1992, 1, 1)), _options).Should().Be("""{"BirthDate":"1992-01-01"}""");
    }
}
