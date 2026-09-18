using System.Buffers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct Color(
	byte R,
	byte G,
	byte B,
	byte A = byte.MaxValue
)
{
	public static Color FromHexString(ReadOnlySpan<char> value)
	{
		var hex = value;
		if (hex[0] is '#')
		{
			hex = hex[1..];
		}

		return hex.Length switch
		{
			3 => new(
				ExpandHexNibble(hex[0]),
				ExpandHexNibble(hex[1]),
				ExpandHexNibble(hex[2])
			),
			4 => new(
				ExpandHexNibble(hex[0]),
				ExpandHexNibble(hex[1]),
				ExpandHexNibble(hex[2]),
				ExpandHexNibble(hex[3])
			),
			6 or 8 => FromHexBytes(hex),
			_ => throw new FormatException("Color hex string must contain 3, 4, 6, or 8 hexadecimal characters.")
		};
	}

	public string ToHexString() => String.Create(A is byte.MaxValue ? 7 : 9, this, static (destination, color) =>
	{
		destination[0] = '#';
		ReadOnlySpan<byte> bytes = [color.R, color.G, color.B, color.A];
		Convert.TryToHexString(color.A is byte.MaxValue ? bytes[..3] : bytes, destination[1..], out _);
	});

	private static Color FromHexBytes(ReadOnlySpan<char> hex)
	{
		Span<byte> bytes = stackalloc byte[4];
		if (Convert.FromHexString(hex, bytes, out _, out var bytesWritten) is not OperationStatus.Done)
		{
			throw new FormatException("Color hex string contains invalid hexadecimal characters.");
		}

		return bytesWritten switch
		{
			3 => new(bytes[0], bytes[1], bytes[2]),
			4 => new(bytes[0], bytes[1], bytes[2], bytes[3]),
			_ => throw new FormatException("Color hex string must contain 3, 4, 6, or 8 hexadecimal characters.")
		};
	}

	private static byte ExpandHexNibble(char value)
	{
		var nibble = ParseHexNibble(value);
		return (byte)((nibble << 4) | nibble);
	}

	private static byte ParseHexNibble(char value) => value switch
	{
		>= '0' and <= '9' => (byte)(value - '0'),
		>= 'a' and <= 'f' => (byte)(value - 'a' + 10),
		>= 'A' and <= 'F' => (byte)(value - 'A' + 10),
		_ => throw new FormatException("Color hex string contains invalid hexadecimal characters.")
	};
}

public sealed class ColorJsonConverter : JsonConverter<Color>
{
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String)
		{
			throw new JsonException($"Unexpected token {reader.TokenType} when parsing {nameof(Color)}.");
		}

		var value = reader.GetString();
		if (value.IsNullOrWhiteSpace())
		{
			throw new JsonException($"Expected a hex color string for {nameof(Color)}.");
		}

		try
		{
			return Color.FromHexString(value);
		}
		catch (FormatException ex)
		{
			throw new JsonException($"Invalid {nameof(Color)} value '{value}'.", ex);
		}
	}

	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value.ToHexString());
}

public static partial class Extensions
{
	extension(string str)
	{
		public Color ToColor() => Color.FromHexString(str);
	}
}