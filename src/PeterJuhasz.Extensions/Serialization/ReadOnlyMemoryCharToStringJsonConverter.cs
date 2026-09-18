namespace System.Text.Json.Serialization;

public class ReadOnlyMemoryCharToStringJsonConverter : JsonConverter<ReadOnlyMemory<char>>
{
	public override ReadOnlyMemory<char> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
	{
		JsonTokenType.String => reader.GetString()!.AsMemory(),
		JsonTokenType.Null => null!,
		_ => throw new JsonException(),
	};

	public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<char> value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Span);
	}
}