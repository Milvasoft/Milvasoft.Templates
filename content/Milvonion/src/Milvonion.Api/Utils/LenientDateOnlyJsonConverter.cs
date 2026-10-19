using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milvonion.Api.Utils;

/// <summary>
/// Reads a calendar date (<see cref="DateOnly"/>, e.g. a date of birth) from "yyyy-MM-dd", and also from a full ISO
/// date-time by the date it was written with: "1992-01-01T00:00:00Z" and "1992-01-01T00:00:00+03:00" are both
/// 1 January 1992. Reading a date-time through a time zone could move it to the day before; a date never goes through
/// a time zone. Writes "yyyy-MM-dd".
/// </summary>
public class LenientDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string _format = "yyyy-MM-dd";

    /// <inheritdoc/>
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;

        // The date part, then nothing or a time ("T..." / " ..."); the time and its offset are not looked at.
        if (text is { Length: >= 10 }
            && (text.Length == 10 || text[10] == 'T' || text[10] == ' ')
            && DateOnly.TryParseExact(text.AsSpan(0, 10), _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        throw new JsonException($"Expected a date as \"{_format}\".");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
}
