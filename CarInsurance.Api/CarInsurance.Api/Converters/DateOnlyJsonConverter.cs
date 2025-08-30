using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarInsurance.Api.Converters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
	private const string Format = "yyyy-MM-dd";

	public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException($"Invalid date. Use {Format}.");

		var s = reader.GetString();
		if (DateOnly.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
			return value;

		throw new JsonException($"Invalid date. Use {Format}.");
	}

	public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}

