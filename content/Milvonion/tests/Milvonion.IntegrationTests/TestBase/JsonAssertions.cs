using FluentAssertions;
using System.Text.Json;

namespace Milvonion.IntegrationTests.TestBase;

/// <summary>
/// Assertions over the raw JSON the API writes. The API omits null properties
/// (<c>DefaultIgnoreCondition = WhenWritingNull</c>), so "the property is absent" and "the property is null" are the
/// same answer on the wire and a test must not care which one it got.
/// </summary>
public static class JsonAssertions
{
    /// <summary>The kind of <paramref name="propertyName"/>, or <see cref="JsonValueKind.Undefined"/> when it was omitted.</summary>
    public static JsonValueKind KindOf(this JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var property) ? property.ValueKind : JsonValueKind.Undefined;

    /// <summary>Asserts that <paramref name="propertyName"/> carries no value (omitted or null).</summary>
    public static void ShouldHaveNoValue(this JsonElement element, string propertyName, string because = "", params object[] becauseArgs)
        => element.KindOf(propertyName).Should().BeOneOf([JsonValueKind.Undefined, JsonValueKind.Null], because, becauseArgs);

    /// <summary>Asserts that <paramref name="propertyName"/> carries a value.</summary>
    public static void ShouldHaveValue(this JsonElement element, string propertyName, string because = "", params object[] becauseArgs)
    {
        var kind = element.KindOf(propertyName);

        kind.Should().NotBe(JsonValueKind.Undefined, because, becauseArgs);
        kind.Should().NotBe(JsonValueKind.Null, because, becauseArgs);
    }

    /// <summary>Asserts that the element itself carries no value: an omitted or null <c>data</c> envelope.</summary>
    public static void ShouldCarryNoData(this JsonElement element, string because = "", params object[] becauseArgs)
        => element.ValueKind.Should().BeOneOf([JsonValueKind.Undefined, JsonValueKind.Null], because, becauseArgs);
}
