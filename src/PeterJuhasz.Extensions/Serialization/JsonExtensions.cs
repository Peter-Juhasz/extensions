using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace System.Text.Json;

public static partial class JsonExtensions
{
	extension(JsonElement jsonElement)
	{
		public JsonElement? GetPropertyOrDefault(string propertyName)
		{
			if (jsonElement is { ValueKind: JsonValueKind.Null })
			{
				return null;
			}

			if (jsonElement.TryGetProperty(propertyName, out var property) && property is not { ValueKind: JsonValueKind.Null })
			{
				return property;
			}

			return null;
		}

		public JsonElement GetRequiredProperty(string propertyName)
		{
			if (jsonElement is not { ValueKind: JsonValueKind.Object })
			{
				throw new InvalidOperationException($"Can't get property '{propertyName}', because the JSON element is a type of '{jsonElement.ValueKind}'.");
			}

			var property = jsonElement.GetPropertyOrDefault(propertyName);
			if (property is null)
			{
				throw new KeyNotFoundException($"Property '{propertyName}' not found.");
			}

			return property.Value;
		}

		public string? GetStringOrDefault()
		{
			if (jsonElement is { ValueKind: JsonValueKind.Null })
			{
				return null;
			}

			return jsonElement.GetString();
		}
	}
}

[RequiresUnreferencedCode("Uses reflection to create generic types at runtime.")]
public class StringValuesJsonConverter : JsonConverter<StringValues>
{
	public override StringValues Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
	{
		JsonTokenType.String => new StringValues(reader.GetString()),
		JsonTokenType.StartArray => new StringValues(JsonSerializer.Deserialize<string[]>(ref reader, options)),
		_ => throw new JsonException(),
	};

	public override void Write(Utf8JsonWriter writer, StringValues value, JsonSerializerOptions options)
	{
		if (value.Count == 1)
		{
			writer.WriteStringValue(value[0]);
		}
		else
		{
			JsonSerializer.Serialize(writer, value.ToArray(), options);
		}
	}
}
